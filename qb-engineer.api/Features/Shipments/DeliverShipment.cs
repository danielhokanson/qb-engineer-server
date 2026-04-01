using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Shipments;

public record DeliverShipmentCommand(int Id) : IRequest;

public class DeliverShipmentHandler(IShipmentRepository repo)
    : IRequestHandler<DeliverShipmentCommand>
{
    public async Task Handle(DeliverShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Shipment {request.Id} not found");

        if (shipment.Status != ShipmentStatus.Shipped && shipment.Status != ShipmentStatus.InTransit)
            throw new InvalidOperationException("Only Shipped or InTransit shipments can be marked delivered");

        shipment.Status = ShipmentStatus.Delivered;
        shipment.DeliveredDate = DateTimeOffset.UtcNow;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
