using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record QcInspectionFailedEvent(int InspectionId, int JobId, int UserId) : INotification;
