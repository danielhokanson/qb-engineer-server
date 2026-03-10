using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetReceivingHistoryQuery(int? PurchaseOrderId, int? PartId, int Take = 50) : IRequest<List<ReceivingRecordResponseModel>>;

public class GetReceivingHistoryHandler(IInventoryRepository repo)
    : IRequestHandler<GetReceivingHistoryQuery, List<ReceivingRecordResponseModel>>
{
    public Task<List<ReceivingRecordResponseModel>> Handle(
        GetReceivingHistoryQuery request, CancellationToken cancellationToken)
        => repo.GetReceivingHistoryAsync(request.PurchaseOrderId, request.PartId, request.Take, cancellationToken);
}
