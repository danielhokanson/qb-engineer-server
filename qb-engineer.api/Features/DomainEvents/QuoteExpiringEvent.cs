using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record QuoteExpiringEvent(int QuoteId, int DaysUntilExpiry, int? AssignedUserId) : INotification;
