using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;
using QBEngineer.Data.Extensions;

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
        var vp = await db.VendorParts
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == request.VendorPartId, ct)
            ?? throw new KeyNotFoundException($"VendorPart {request.VendorPartId} not found");

        var body = request.Body;

        var existing = await db.VendorPartPriceTiers.FirstOrDefaultAsync(
            t => t.VendorPartId == request.VendorPartId
                && t.MinQuantity == body.MinQuantity
                && t.EffectiveFrom == body.EffectiveFrom,
            ct);

        VendorPartPriceTier tier;
        bool isNew;
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
            isNew = true;
        }
        else
        {
            existing.UnitPrice = body.UnitPrice;
            existing.Currency = body.Currency.Trim().ToUpperInvariant();
            existing.EffectiveTo = body.EffectiveTo;
            existing.Notes = body.Notes;
            tier = existing;
            isNew = false;
        }

        // Indexing-points rule: a price tier is data on the VendorPart, which
        // bridges Part ↔ Vendor — log on both. Description includes the
        // tier's defining triple so future readers can match the audit row
        // back to the data point without joining.
        var vendorName = vp.Vendor?.CompanyName ?? "(unknown vendor)";
        var verb = isNew ? "added" : "updated";
        var summary = $"Price tier {verb} for {vendorName}: qty ≥ {tier.MinQuantity} @ {tier.UnitPrice:0.##} {tier.Currency} (effective {tier.EffectiveFrom:yyyy-MM-dd})";
        db.LogActivityAt(
            isNew ? "price-tier-added" : "price-tier-updated",
            summary,
            ("Part", vp.PartId),
            ("Vendor", vp.VendorId));

        await db.SaveChangesAsync(ct);

        return VendorPartMapper.ToTierResponse(tier);
    }
}
