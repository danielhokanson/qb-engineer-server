using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetAtpTimelineQuery(int PartId, DateOnly? From, DateOnly? To) : IRequest<List<AtpBucket>>;

public class GetAtpTimelineHandler(IAtpService atpService) : IRequestHandler<GetAtpTimelineQuery, List<AtpBucket>>
{
    public async Task<List<AtpBucket>> Handle(GetAtpTimelineQuery request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var to = request.To ?? from.AddDays(90);

        return await atpService.GetAtpTimelineAsync(request.PartId, from, to, cancellationToken);
    }
}
