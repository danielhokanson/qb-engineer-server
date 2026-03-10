using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Quality;

public record GetQcInspectionsQuery(int? JobId, string? Status, string? LotNumber) : IRequest<List<QcInspectionResponseModel>>;

public class GetQcInspectionsHandler(AppDbContext db)
    : IRequestHandler<GetQcInspectionsQuery, List<QcInspectionResponseModel>>
{
    public async Task<List<QcInspectionResponseModel>> Handle(
        GetQcInspectionsQuery request, CancellationToken cancellationToken)
    {
        var query = db.QcInspections
            .AsNoTracking()
            .Include(i => i.Results)
            .Include(i => i.Job)
            .Include(i => i.Template)
            .AsQueryable();

        if (request.JobId.HasValue)
            query = query.Where(i => i.JobId == request.JobId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(i => i.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.LotNumber))
            query = query.Where(i => i.LotNumber != null && i.LotNumber.Contains(request.LotNumber));

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new QcInspectionResponseModel(
                i.Id,
                i.JobId,
                i.Job != null ? i.Job.JobNumber : null,
                i.ProductionRunId,
                i.TemplateId,
                i.Template != null ? i.Template.Name : null,
                i.InspectorId,
                db.Users.Where(u => u.Id == i.InspectorId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? "",
                i.LotNumber,
                i.Status,
                i.Notes,
                i.CompletedAt,
                i.Results.Select(r => new QcInspectionResultModel(
                    r.Id,
                    r.ChecklistItemId,
                    r.Description,
                    r.Passed,
                    r.MeasuredValue,
                    r.Notes
                )).ToList(),
                i.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
