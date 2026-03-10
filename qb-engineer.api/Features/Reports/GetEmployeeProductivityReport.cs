using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetEmployeeProductivityReportQuery(DateTimeOffset Start, DateTimeOffset End) : IRequest<List<EmployeeProductivityReportItem>>;

public class GetEmployeeProductivityReportHandler(IReportRepository repo) : IRequestHandler<GetEmployeeProductivityReportQuery, List<EmployeeProductivityReportItem>>
{
    public Task<List<EmployeeProductivityReportItem>> Handle(GetEmployeeProductivityReportQuery request, CancellationToken cancellationToken)
    {
        return repo.GetEmployeeProductivityAsync(request.Start, request.End, cancellationToken);
    }
}
