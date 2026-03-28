using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Parts;

public record AddPartPriceCommand(int PartId, decimal UnitPrice, DateTime? EffectiveFrom, string? Notes) : IRequest<PartPriceResponseModel>;

public class AddPartPriceHandler(AppDbContext db)
    : IRequestHandler<AddPartPriceCommand, PartPriceResponseModel>
{
    public async Task<PartPriceResponseModel> Handle(AddPartPriceCommand request, CancellationToken ct)
    {
        var effectiveFrom = (request.EffectiveFrom ?? DateTime.UtcNow).ToUniversalTime();

        // End-date any currently active price that starts before the new effective date
        var active = await db.PartPrices
            .Where(p => p.PartId == request.PartId && p.EffectiveTo == null && p.EffectiveFrom < effectiveFrom)
            .ToListAsync(ct);

        foreach (var price in active)
            price.EffectiveTo = effectiveFrom;

        var newPrice = new PartPrice
        {
            PartId = request.PartId,
            UnitPrice = request.UnitPrice,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            Notes = request.Notes,
        };

        db.PartPrices.Add(newPrice);
        await db.SaveChangesAsync(ct);

        return new PartPriceResponseModel(
            newPrice.Id,
            newPrice.PartId,
            newPrice.UnitPrice,
            newPrice.EffectiveFrom,
            newPrice.EffectiveTo,
            newPrice.Notes,
            IsCurrent: true);
    }
}
