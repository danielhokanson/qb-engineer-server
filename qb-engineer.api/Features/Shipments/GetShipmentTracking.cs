using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Shipments;

public record GetShipmentTrackingQuery(int ShipmentId) : IRequest<ShipmentTracking?>;

public class GetShipmentTrackingHandler(
    IShipmentRepository shipmentRepo,
    IShippingService shippingService)
    : IRequestHandler<GetShipmentTrackingQuery, ShipmentTracking?>
{
    public async Task<ShipmentTracking?> Handle(GetShipmentTrackingQuery request, CancellationToken cancellationToken)
    {
        var shipment = await shipmentRepo.FindAsync(request.ShipmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment {request.ShipmentId} not found");

        if (string.IsNullOrEmpty(shipment.TrackingNumber))
            return null;

        return await shippingService.GetTrackingAsync(shipment.TrackingNumber, cancellationToken);
    }
}
