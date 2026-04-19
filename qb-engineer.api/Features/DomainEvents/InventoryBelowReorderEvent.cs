using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record InventoryBelowReorderEvent(int PartId, int CurrentQty, int ReorderPoint) : INotification;
