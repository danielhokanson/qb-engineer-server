using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record SalesOrderConfirmedEvent(int SalesOrderId, int UserId) : INotification;
