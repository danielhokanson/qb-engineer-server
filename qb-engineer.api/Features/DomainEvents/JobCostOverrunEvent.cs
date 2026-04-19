using MediatR;

namespace QBEngineer.Api.Features.DomainEvents;

public record JobCostOverrunEvent(int JobId, decimal EstimatedCost, decimal ActualCost, decimal VariancePercent) : INotification;
