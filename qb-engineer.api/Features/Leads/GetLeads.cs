using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Leads;

public record GetLeadsQuery(LeadStatus? Status, string? Search) : IRequest<List<LeadResponseModel>>;

public class GetLeadsHandler(ILeadRepository repo) : IRequestHandler<GetLeadsQuery, List<LeadResponseModel>>
{
    public Task<List<LeadResponseModel>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
        => repo.GetLeadsAsync(request.Status, request.Search, cancellationToken);
}
