using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Workflows;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Workflows.Runs;

public record GetWorkflowRunQuery(int RunId) : IRequest<WorkflowRunResponseModel>;

public class GetWorkflowRunHandler(AppDbContext db)
    : IRequestHandler<GetWorkflowRunQuery, WorkflowRunResponseModel>
{
    public async Task<WorkflowRunResponseModel> Handle(GetWorkflowRunQuery request, CancellationToken ct)
    {
        var run = await db.WorkflowRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RunId, ct)
            ?? throw new KeyNotFoundException($"Workflow run id {request.RunId} not found.");
        return run.ToResponse();
    }
}
