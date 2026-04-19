using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record CustomerReturnReceivedEvent(int ReturnId, int SalesOrderId, int UserId) : INotification;
