using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

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
            .FirstOrDefaultAsync(t => t.Id == request.TierId && t.VendorPartId == request.VendorPartId, ct)
            ?? throw new KeyNotFoundException(
                $"Price tier {request.TierId} not found on VendorPart {request.VendorPartId}");

        db.VendorPartPriceTiers.Remove(tier);
        await db.SaveChangesAsync(ct);
    }
}
