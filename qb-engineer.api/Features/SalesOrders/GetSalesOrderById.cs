using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.SalesOrders;

public record GetSalesOrderByIdQuery(int Id) : IRequest<SalesOrderDetailResponseModel>;

public class GetSalesOrderByIdHandler(ISalesOrderRepository repo)
    : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDetailResponseModel>
{
    public async Task<SalesOrderDetailResponseModel> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var so = await repo.FindWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order {request.Id} not found");

        return new SalesOrderDetailResponseModel(
            so.Id,
            so.OrderNumber,
            so.CustomerId,
            so.Customer.Name,
            so.QuoteId,
            so.Quote?.QuoteNumber,
            so.ShippingAddressId,
            so.BillingAddressId,
            so.Status.ToString(),
            so.CreditTerms?.ToString(),
            so.ConfirmedDate,
            so.RequestedDeliveryDate,
            so.CustomerPO,
            so.Notes,
            so.TaxRate,
            so.Lines.Sum(l => l.Quantity * l.UnitPrice),
            so.Lines.Sum(l => l.Quantity * l.UnitPrice) * so.TaxRate,
            so.Lines.Sum(l => l.Quantity * l.UnitPrice) * (1 + so.TaxRate),
            so.Lines.Select(l => new SalesOrderLineResponseModel(
                l.Id,
                l.PartId,
                l.Part?.PartNumber,
                l.Description,
                l.Quantity,
                l.UnitPrice,
                l.Quantity * l.UnitPrice,
                l.LineNumber,
                l.ShippedQuantity,
                l.RemainingQuantity,
                l.IsFullyShipped,
                l.Notes)).ToList(),
            so.CreatedAt,
            so.UpdatedAt);
    }
}
