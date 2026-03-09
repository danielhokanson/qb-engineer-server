using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Terminology;

public record GetTerminologyQuery : IRequest<List<TerminologyEntryResponseModel>>;

public class GetTerminologyHandler(ITerminologyRepository repo)
    : IRequestHandler<GetTerminologyQuery, List<TerminologyEntryResponseModel>>
{
    public async Task<List<TerminologyEntryResponseModel>> Handle(
        GetTerminologyQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(cancellationToken);
    }
}
