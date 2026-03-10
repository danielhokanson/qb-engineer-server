using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs.Parts;

public record GetJobPartsQuery(int JobId) : IRequest<List<JobPartResponseModel>>;

public class GetJobPartsHandler(AppDbContext db) : IRequestHandler<GetJobPartsQuery, List<JobPartResponseModel>>
{
    public async Task<List<JobPartResponseModel>> Handle(GetJobPartsQuery request, CancellationToken cancellationToken)
    {
        return await db.JobParts
            .Where(jp => jp.JobId == request.JobId)
            .Include(jp => jp.Part)
            .Select(jp => new JobPartResponseModel(
                jp.Id,
                jp.JobId,
                jp.PartId,
                jp.Part.PartNumber,
                jp.Part.Description,
                jp.Part.Status.ToString(),
                jp.Quantity,
                jp.Notes))
            .ToListAsync(cancellationToken);
    }
}
