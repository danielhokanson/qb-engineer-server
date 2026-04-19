using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record DeliveryDateChangedEvent(int SalesOrderLineId, DateTimeOffset OldDate, DateTimeOffset NewDate, int UserId) : INotification;
