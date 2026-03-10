using FluentValidation;
using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.SalesOrders;

public record UpdateSalesOrderCommand(
    int Id,
    int? ShippingAddressId,
    int? BillingAddressId,
    string? CreditTerms,
    DateTime? RequestedDeliveryDate,
    string? CustomerPO,
    string? Notes,
    decimal? TaxRate) : IRequest;

public class UpdateSalesOrderValidator : AbstractValidator<UpdateSalesOrderCommand>
{
    public UpdateSalesOrderValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ShippingAddressId).GreaterThan(0).When(x => x.ShippingAddressId.HasValue);
        RuleFor(x => x.BillingAddressId).GreaterThan(0).When(x => x.BillingAddressId.HasValue);
        RuleFor(x => x.CreditTerms).MaximumLength(50).When(x => x.CreditTerms is not null);
        RuleFor(x => x.CustomerPO).MaximumLength(100).When(x => x.CustomerPO is not null);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 1).When(x => x.TaxRate.HasValue);
    }
}

public class UpdateSalesOrderHandler(ISalesOrderRepository repo)
    : IRequestHandler<UpdateSalesOrderCommand>
{
    public async Task Handle(UpdateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order {request.Id} not found");

        if (order.Status != SalesOrderStatus.Draft && order.Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException("Can only update Draft or Confirmed sales orders");

        if (request.ShippingAddressId.HasValue) order.ShippingAddressId = request.ShippingAddressId;
        if (request.BillingAddressId.HasValue) order.BillingAddressId = request.BillingAddressId;
        if (request.CreditTerms != null) order.CreditTerms = Enum.Parse<CreditTerms>(request.CreditTerms, true);
        if (request.RequestedDeliveryDate.HasValue) order.RequestedDeliveryDate = request.RequestedDeliveryDate;
        if (request.CustomerPO != null) order.CustomerPO = request.CustomerPO;
        if (request.Notes != null) order.Notes = request.Notes;
        if (request.TaxRate.HasValue) order.TaxRate = request.TaxRate.Value;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
