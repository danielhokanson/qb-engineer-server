using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Estimates;

public record GetEstimateQuery(int Id) : IRequest<EstimateDetailResponseModel>;

public class GetEstimateHandler(AppDbContext db) : IRequestHandler<GetEstimateQuery, EstimateDetailResponseModel>
{
    public async Task<EstimateDetailResponseModel> Handle(GetEstimateQuery request, CancellationToken ct)
    {
        var e = await db.Estimates
            .AsNoTracking()
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"Estimate {request.Id} not found.");

        string? assignedToName = null;
        if (e.AssignedToId.HasValue)
        {
            assignedToName = await db.Users
                .Where(u => u.Id == e.AssignedToId.Value)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstOrDefaultAsync(ct);
        }

        return new EstimateDetailResponseModel(
            e.Id,
            e.CustomerId,
            e.Customer.Name,
            e.Title,
            e.Description,
            e.EstimatedAmount,
            e.Status,
            e.ValidUntil,
            e.Notes,
            e.AssignedToId,
            assignedToName,
            e.ConvertedToQuoteId,
            e.ConvertedAt,
            e.CreatedAt,
            e.UpdatedAt);
    }
}
