using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record JobCreatedEvent(int JobId, int UserId) : INotification;
