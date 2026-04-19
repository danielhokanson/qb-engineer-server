using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record InvoicePastDueEvent(int InvoiceId, int CustomerId, int DaysOverdue) : INotification;
