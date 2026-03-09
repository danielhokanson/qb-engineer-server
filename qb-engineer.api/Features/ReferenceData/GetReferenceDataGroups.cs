using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.ReferenceData;

public record GetReferenceDataGroupsQuery : IRequest<List<ReferenceDataGroupResponseModel>>;

public class GetReferenceDataGroupsHandler(IReferenceDataRepository repo) : IRequestHandler<GetReferenceDataGroupsQuery, List<ReferenceDataGroupResponseModel>>
{
    public Task<List<ReferenceDataGroupResponseModel>> Handle(GetReferenceDataGroupsQuery request, CancellationToken cancellationToken)
        => repo.GetAllGroupsAsync(cancellationToken);
}
