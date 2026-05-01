using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.VendorParts;

/// <summary>
/// Pillar 3 — Upsert a tiered-price row on a VendorPart. Upsert key is
/// (VendorPartId, MinQuantity, EffectiveFrom). Re-posting the same triple
/// updates the matched row's UnitPrice / Currency / EffectiveTo / Notes;
/// a unique triple inserts a new row.
/// </summary>
public record UpsertVendorPartPriceTierCommand(int VendorPartId, UpsertVendorPartPriceTierRequestModel Body)
    : IRequest<VendorPartPriceTierResponseModel>;

public class UpsertVendorPartPriceTierValidator : AbstractValidator<UpsertVendorPartPriceTierCommand>
{
    public UpsertVendorPartPriceTierValidator()
    {
        RuleFor(x => x.VendorPartId).GreaterThan(0);
        RuleFor(x => x.Body.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Body.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Body.Notes).MaximumLength(2000);
        RuleFor(x => x.Body)
            .Must(b => !b.EffectiveTo.HasValue || b.EffectiveTo.Value >= b.EffectiveFrom)
            .WithMessage("EffectiveTo must be on or after EffectiveFrom.");
    }
}

public class UpsertVendorPartPriceTierHandler(AppDbContext db)
    : IRequestHandler<UpsertVendorPartPriceTierCommand, VendorPartPriceTierResponseModel>
{
    public async Task<VendorPartPriceTierResponseModel> Handle(UpsertVendorPartPriceTierCommand request, CancellationToken ct)
    {
        var vendorPartExists = await db.VendorParts.AnyAsync(x => x.Id == request.VendorPartId, ct);
        if (!vendorPartExists)
            throw new KeyNotFoundException($"VendorPart {request.VendorPartId} not found");

        var body = request.Body;

        var existing = await db.VendorPartPriceTiers.FirstOrDefaultAsync(
            t => t.VendorPartId == request.VendorPartId
                && t.MinQuantity == body.MinQuantity
                && t.EffectiveFrom == body.EffectiveFrom,
            ct);

        VendorPartPriceTier tier;
        if (existing is null)
        {
            tier = new VendorPartPriceTier
            {
                VendorPartId = request.VendorPartId,
                MinQuantity = body.MinQuantity,
                UnitPrice = body.UnitPrice,
                Currency = body.Currency.Trim().ToUpperInvariant(),
                EffectiveFrom = body.EffectiveFrom,
                EffectiveTo = body.EffectiveTo,
                Notes = body.Notes,
            };
            db.VendorPartPriceTiers.Add(tier);
        }
        else
        {
            existing.UnitPrice = body.UnitPrice;
            existing.Currency = body.Currency.Trim().ToUpperInvariant();
            existing.EffectiveTo = body.EffectiveTo;
            existing.Notes = body.Notes;
            tier = existing;
        }

        await db.SaveChangesAsync(ct);

        return VendorPartMapper.ToTierResponse(tier);
    }
}
