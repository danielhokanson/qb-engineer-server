using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.TrackTypes;

public record GetTrackTypesQuery : IRequest<List<TrackTypeDto>>;

public class GetTrackTypesHandler(AppDbContext db) : IRequestHandler<GetTrackTypesQuery, List<TrackTypeDto>>
{
    public async Task<List<TrackTypeDto>> Handle(GetTrackTypesQuery request, CancellationToken cancellationToken)
    {
        return await db.TrackTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Select(t => new TrackTypeDto(
                t.Id,
                t.Name,
                t.Code,
                t.Description,
                t.IsDefault,
                t.SortOrder,
                t.Stages
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new StageDto(
                        s.Id,
                        s.Name,
                        s.Code,
                        s.SortOrder,
                        s.Color,
                        s.WIPLimit,
                        s.AccountingDocumentType != null ? s.AccountingDocumentType.ToString() : null,
                        s.IsIrreversible))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
