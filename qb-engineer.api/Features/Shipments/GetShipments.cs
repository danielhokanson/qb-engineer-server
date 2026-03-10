using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Shipments;

public record GetShipmentsQuery(int? SalesOrderId, ShipmentStatus? Status) : IRequest<List<ShipmentListItemModel>>;

public class GetShipmentsHandler(IShipmentRepository repo)
    : IRequestHandler<GetShipmentsQuery, List<ShipmentListItemModel>>
{
    public async Task<List<ShipmentListItemModel>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.SalesOrderId, request.Status, cancellationToken);
    }
}
