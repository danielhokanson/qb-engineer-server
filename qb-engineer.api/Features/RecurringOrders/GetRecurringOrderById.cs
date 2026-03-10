using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.RecurringOrders;

public record GetRecurringOrderByIdQuery(int Id) : IRequest<RecurringOrderDetailResponseModel>;

public class GetRecurringOrderByIdHandler(IRecurringOrderRepository repo)
    : IRequestHandler<GetRecurringOrderByIdQuery, RecurringOrderDetailResponseModel>
{
    public async Task<RecurringOrderDetailResponseModel> Handle(GetRecurringOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var ro = await repo.FindWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Recurring order {request.Id} not found");

        return new RecurringOrderDetailResponseModel(
            ro.Id, ro.Name, ro.CustomerId, ro.Customer.Name,
            ro.ShippingAddressId, ro.IntervalDays, ro.NextGenerationDate,
            ro.LastGeneratedDate, ro.IsActive, ro.Notes,
            ro.Lines.OrderBy(l => l.LineNumber).Select(l => new RecurringOrderLineResponseModel(
                l.Id, l.PartId, l.Part.PartNumber, l.Description,
                l.Quantity, l.UnitPrice, l.LineNumber)).ToList(),
            ro.CreatedAt, ro.UpdatedAt);
    }
}
