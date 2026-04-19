using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record JobStageChangedEvent(int JobId, int FromStageId, int ToStageId, int UserId) : INotification;
