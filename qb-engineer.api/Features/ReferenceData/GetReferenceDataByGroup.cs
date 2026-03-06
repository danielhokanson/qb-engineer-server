using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ReferenceData;

public record GetReferenceDataByGroupQuery(string GroupCode) : IRequest<List<ReferenceDataDto>>;

public class GetReferenceDataByGroupHandler(AppDbContext db) : IRequestHandler<GetReferenceDataByGroupQuery, List<ReferenceDataDto>>
{
    public async Task<List<ReferenceDataDto>> Handle(GetReferenceDataByGroupQuery request, CancellationToken cancellationToken)
    {
        return await db.ReferenceData
            .Where(r => r.GroupCode == request.GroupCode)
            .OrderBy(r => r.SortOrder)
            .Select(r => new ReferenceDataDto(
                r.Id,
                r.Code,
                r.Label,
                r.SortOrder,
                r.IsActive,
                r.Metadata))
            .ToListAsync(cancellationToken);
    }
}
