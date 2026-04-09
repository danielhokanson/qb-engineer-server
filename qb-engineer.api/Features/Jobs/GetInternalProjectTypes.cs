using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record GetInternalProjectTypesQuery() : IRequest<List<ReferenceDataResponseModel>>;

public class GetInternalProjectTypesHandler(AppDbContext db)
    : IRequestHandler<GetInternalProjectTypesQuery, List<ReferenceDataResponseModel>>
{
    public async Task<List<ReferenceDataResponseModel>> Handle(GetInternalProjectTypesQuery request, CancellationToken ct)
    {
        return await db.ReferenceData
            .Where(r => r.GroupCode == "internal_project_type" && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .Select(r => new ReferenceDataResponseModel(r.Id, r.Code, r.Label, r.SortOrder, r.IsActive, r.IsSeedData, r.Metadata))
            .ToListAsync(ct);
    }
}
