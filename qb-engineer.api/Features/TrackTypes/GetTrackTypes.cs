using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TrackTypes;

public record GetTrackTypesQuery : IRequest<List<TrackTypeResponseModel>>;

public class GetTrackTypesHandler(ITrackTypeRepository repo) : IRequestHandler<GetTrackTypesQuery, List<TrackTypeResponseModel>>
{
    public Task<List<TrackTypeResponseModel>> Handle(GetTrackTypesQuery request, CancellationToken cancellationToken)
        => repo.GetAllAsync(cancellationToken);
}
