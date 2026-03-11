using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.StatusTracking;

public record GetStatusHistoryQuery(string EntityType, int EntityId) : IRequest<List<StatusEntryResponseModel>>;

public class GetStatusHistoryHandler(IStatusEntryRepository repository)
    : IRequestHandler<GetStatusHistoryQuery, List<StatusEntryResponseModel>>
{
    public Task<List<StatusEntryResponseModel>> Handle(
        GetStatusHistoryQuery request, CancellationToken cancellationToken)
        => repository.GetHistoryAsync(request.EntityType, request.EntityId, cancellationToken);
}
