using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.SalesOrders;

public record CreateSalesOrderCommand(
    int CustomerId,
    int? QuoteId,
    int? ShippingAddressId,
    int? BillingAddressId,
    string? CreditTerms,
    DateTime? RequestedDeliveryDate,
    string? CustomerPO,
    string? Notes,
    decimal TaxRate,
    List<CreateSalesOrderLineModel> Lines) : IRequest<SalesOrderListItemModel>;

public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).LessThan(1);
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateSalesOrderHandler(ISalesOrderRepository repo, ICustomerRepository customerRepo, IBarcodeService barcodeService)
    : IRequestHandler<CreateSalesOrderCommand, SalesOrderListItemModel>
{
    public async Task<SalesOrderListItemModel> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepo.FindAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        var orderNumber = await repo.GenerateNextOrderNumberAsync(cancellationToken);

        CreditTerms? creditTerms = request.CreditTerms != null
            ? Enum.Parse<CreditTerms>(request.CreditTerms, true)
            : null;

        var order = new SalesOrder
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            QuoteId = request.QuoteId,
            ShippingAddressId = request.ShippingAddressId,
            BillingAddressId = request.BillingAddressId,
            CreditTerms = creditTerms,
            RequestedDeliveryDate = request.RequestedDeliveryDate,
            CustomerPO = request.CustomerPO,
            Notes = request.Notes,
            TaxRate = request.TaxRate,
        };

        var lineNumber = 1;
        foreach (var line in request.Lines)
        {
            order.Lines.Add(new SalesOrderLine
            {
                PartId = line.PartId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineNumber = lineNumber++,
                Notes = line.Notes,
            });
        }

        await repo.AddAsync(order, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        await barcodeService.CreateBarcodeAsync(
            BarcodeEntityType.SalesOrder, order.Id, order.OrderNumber, cancellationToken);

        var total = order.Lines.Sum(l => l.Quantity * l.UnitPrice);

        return new SalesOrderListItemModel(
            order.Id, order.OrderNumber, order.CustomerId, customer.Name,
            order.Status.ToString(), order.CustomerPO, order.Lines.Count,
            total, order.RequestedDeliveryDate, order.CreatedAt);
    }
}
