using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ReferenceData;

public record GetReferenceDataGroupsQuery : IRequest<List<ReferenceDataGroupDto>>;

public class GetReferenceDataGroupsHandler(AppDbContext db) : IRequestHandler<GetReferenceDataGroupsQuery, List<ReferenceDataGroupDto>>
{
    public async Task<List<ReferenceDataGroupDto>> Handle(GetReferenceDataGroupsQuery request, CancellationToken cancellationToken)
    {
        var allData = await db.ReferenceData
            .OrderBy(r => r.GroupCode)
            .ThenBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);

        return allData
            .GroupBy(r => r.GroupCode)
            .Select(g => new ReferenceDataGroupDto(
                g.Key,
                g.Select(r => new ReferenceDataDto(
                    r.Id,
                    r.Code,
                    r.Label,
                    r.SortOrder,
                    r.IsActive,
                    r.Metadata))
                .ToList()))
            .ToList();
    }
}
