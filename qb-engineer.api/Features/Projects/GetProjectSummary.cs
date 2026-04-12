using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Projects;

public record GetProjectSummaryQuery(int Id) : IRequest<ProjectSummaryResponseModel>;

public class GetProjectSummaryHandler(IProjectAccountingService projectService) : IRequestHandler<GetProjectSummaryQuery, ProjectSummaryResponseModel>
{
    public async Task<ProjectSummaryResponseModel> Handle(GetProjectSummaryQuery query, CancellationToken cancellationToken)
    {
        return await projectService.GetProjectSummaryAsync(query.Id, cancellationToken);
    }
}
