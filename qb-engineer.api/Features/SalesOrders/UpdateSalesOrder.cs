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
