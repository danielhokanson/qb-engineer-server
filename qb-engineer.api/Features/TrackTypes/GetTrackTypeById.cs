using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.TrackTypes;

public record GetTrackTypeByIdQuery(int Id) : IRequest<TrackTypeDto>;

public class GetTrackTypeByIdHandler(AppDbContext db) : IRequestHandler<GetTrackTypeByIdQuery, TrackTypeDto>
{
    public async Task<TrackTypeDto> Handle(GetTrackTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var trackType = await db.TrackTypes
            .Where(t => t.Id == request.Id && t.IsActive)
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
            .FirstOrDefaultAsync(cancellationToken);

        return trackType ?? throw new KeyNotFoundException($"Track type with ID {request.Id} not found.");
    }
}
