using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Leads;

public record GetLeadByIdQuery(int Id) : IRequest<LeadResponseModel?>;

public class GetLeadByIdHandler(ILeadRepository repo) : IRequestHandler<GetLeadByIdQuery, LeadResponseModel?>
{
    public Task<LeadResponseModel?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
        => repo.GetByIdAsync(request.Id, cancellationToken);
}
