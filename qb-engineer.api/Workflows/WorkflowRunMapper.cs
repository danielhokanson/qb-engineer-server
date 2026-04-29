using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Workflows;

internal static class WorkflowRunMapper
{
    public static WorkflowRunResponseModel ToResponse(this WorkflowRun row) => new(
        row.Id,
        row.EntityType,
        row.EntityId,
        row.DefinitionId,
        row.CurrentStepId,
        row.Mode,
        row.StartedAt,
        row.StartedByUserId,
        row.CompletedAt,
        row.AbandonedAt,
        row.AbandonedReason,
        row.LastActivityAt,
        row.Version);
}
