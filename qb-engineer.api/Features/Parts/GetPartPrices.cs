using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Parts;

public record GetPartPricesQuery(int PartId) : IRequest<List<PartPriceResponseModel>>;

public class GetPartPricesHandler(AppDbContext db)
    : IRequestHandler<GetPartPricesQuery, List<PartPriceResponseModel>>
{
    public async Task<List<PartPriceResponseModel>> Handle(
        GetPartPricesQuery request, CancellationToken ct)
    {
        var prices = await db.PartPrices
            .AsNoTracking()
            .Where(p => p.PartId == request.PartId)
            .OrderByDescending(p => p.EffectiveFrom)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        return prices.Select(p => new PartPriceResponseModel(
            p.Id,
            p.PartId,
            p.UnitPrice,
            p.EffectiveFrom,
            p.EffectiveTo,
            p.Notes,
            IsCurrent: p.EffectiveTo == null && p.EffectiveFrom <= now)).ToList();
    }
}
