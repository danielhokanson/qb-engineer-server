using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record ShipmentDeliveredEvent(int ShipmentId, int SalesOrderId, int UserId) : INotification;
