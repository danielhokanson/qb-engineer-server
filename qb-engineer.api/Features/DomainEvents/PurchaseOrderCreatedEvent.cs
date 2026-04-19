using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record PurchaseOrderCreatedEvent(int PurchaseOrderId, int UserId) : INotification;
