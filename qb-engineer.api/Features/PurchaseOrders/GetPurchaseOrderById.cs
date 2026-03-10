using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record GetPurchaseOrderByIdQuery(int Id) : IRequest<PurchaseOrderDetailResponseModel>;

public class GetPurchaseOrderByIdHandler(IPurchaseOrderRepository repo)
    : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailResponseModel>
{
    public async Task<PurchaseOrderDetailResponseModel> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await repo.FindWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found");

        return new PurchaseOrderDetailResponseModel(
            po.Id,
            po.PONumber,
            po.VendorId,
            po.Vendor.CompanyName,
            po.JobId,
            po.Job?.JobNumber,
            po.Status.ToString(),
            po.SubmittedDate,
            po.AcknowledgedDate,
            po.ExpectedDeliveryDate,
            po.ReceivedDate,
            po.Notes,
            po.Lines.Select(l => new PurchaseOrderLineResponseModel(
                l.Id,
                l.PartId,
                l.Part.PartNumber,
                l.Description,
                l.OrderedQuantity,
                l.ReceivedQuantity,
                l.RemainingQuantity,
                l.UnitPrice,
                l.OrderedQuantity * l.UnitPrice,
                l.Notes)).ToList(),
            po.CreatedAt,
            po.UpdatedAt);
    }
}
