using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;
using QBEngineer.Data.Extensions;

namespace QBEngineer.Api.Features.VendorParts;

/// <summary>
/// Pillar 3 — Hard-delete a price tier row. <see cref="Core.Entities.VendorPartPriceTier"/>
/// extends BaseEntity (not BaseAuditableEntity), and the entity intentionally
/// has no soft-delete column — a removed tier is just gone.
/// </summary>
public record DeleteVendorPartPriceTierCommand(int VendorPartId, int TierId) : IRequest;

public class DeleteVendorPartPriceTierHandler(AppDbContext db)
    : IRequestHandler<DeleteVendorPartPriceTierCommand>
{
    public async Task Handle(DeleteVendorPartPriceTierCommand request, CancellationToken ct)
    {
        var tier = await db.VendorPartPriceTiers
            .Include(t => t.VendorPart)
                .ThenInclude(vp => vp.Vendor)
            .FirstOrDefaultAsync(t => t.Id == request.TierId && t.VendorPartId == request.VendorPartId, ct)
            ?? throw new KeyNotFoundException(
                $"Price tier {request.TierId} not found on VendorPart {request.VendorPartId}");

        var vp = tier.VendorPart;
        var vendorName = vp.Vendor?.CompanyName ?? "(unknown vendor)";
        var summary = $"Price tier removed for {vendorName}: qty ≥ {tier.MinQuantity} @ {tier.UnitPrice:0.##} {tier.Currency}";

        db.VendorPartPriceTiers.Remove(tier);

        // Indexing-points rule: tiers are vendor-pricing data on a VendorPart
        // which bridges Part ↔ Vendor — log on both.
        db.LogActivityAt(
            "price-tier-removed",
            summary,
            ("Part", vp.PartId),
            ("Vendor", vp.VendorId));

        await db.SaveChangesAsync(ct);
    }
}
