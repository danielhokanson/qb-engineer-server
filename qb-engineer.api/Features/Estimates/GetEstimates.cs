using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Estimates;

public record GetEstimatesQuery(
    int? CustomerId = null,
    EstimateStatus? Status = null) : IRequest<List<EstimateListItemModel>>;

public class GetEstimatesHandler(AppDbContext db) : IRequestHandler<GetEstimatesQuery, List<EstimateListItemModel>>
{
    public async Task<List<EstimateListItemModel>> Handle(GetEstimatesQuery request, CancellationToken ct)
    {
        var query = db.Estimates
            .AsNoTracking()
            .Where(e => e.DeletedAt == null)
            .AsQueryable();

        if (request.CustomerId.HasValue)
            query = query.Where(e => e.CustomerId == request.CustomerId.Value);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EstimateListItemModel(
                e.Id,
                e.CustomerId,
                e.Customer.Name,
                e.Title,
                e.EstimatedAmount,
                e.Status,
                e.ValidUntil,
                e.ConvertedToQuoteId,
                e.AssignedToId != null
                    ? db.Users.Where(u => u.Id == e.AssignedToId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault()
                    : null,
                e.CreatedAt))
            .ToListAsync(ct);
    }
}
