using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetLeadPipelineReportQuery : IRequest<List<LeadPipelineReportItem>>;

public class GetLeadPipelineReportHandler(IReportRepository repo) : IRequestHandler<GetLeadPipelineReportQuery, List<LeadPipelineReportItem>>
{
    public Task<List<LeadPipelineReportItem>> Handle(GetLeadPipelineReportQuery request, CancellationToken cancellationToken)
    {
        return repo.GetLeadPipelineAsync(cancellationToken);
    }
}
