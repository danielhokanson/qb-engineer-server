using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.VendorParts;

/// <summary>
/// Pillar 3 — Update sourcing metadata on an existing VendorPart. VendorId /
/// PartId are immutable post-create. Sets the at-most-one-preferred-per-Part
/// invariant identically to <see cref="CreateVendorPartHandler"/>.
/// </summary>
public record UpdateVendorPartCommand(int Id, UpdateVendorPartRequestModel Body)
    : IRequest<VendorPartResponseModel>;

public class UpdateVendorPartValidator : AbstractValidator<UpdateVendorPartCommand>
{
    public UpdateVendorPartValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Body.VendorPartNumber).MaximumLength(100);
        RuleFor(x => x.Body.ManufacturerName).MaximumLength(200);
        RuleFor(x => x.Body.VendorMpn).MaximumLength(100);
        RuleFor(x => x.Body.CountryOfOrigin).MaximumLength(2);
        RuleFor(x => x.Body.HtsCode).MaximumLength(20);
        RuleFor(x => x.Body.Notes).MaximumLength(2000);
        RuleFor(x => x.Body.LeadTimeDays).GreaterThanOrEqualTo(0).When(x => x.Body.LeadTimeDays.HasValue);
        RuleFor(x => x.Body.MinOrderQty).GreaterThanOrEqualTo(0).When(x => x.Body.MinOrderQty.HasValue);
        RuleFor(x => x.Body.PackSize).GreaterThanOrEqualTo(0).When(x => x.Body.PackSize.HasValue);
    }
}

public class UpdateVendorPartHandler(AppDbContext db)
    : IRequestHandler<UpdateVendorPartCommand, VendorPartResponseModel>
{
    public async Task<VendorPartResponseModel> Handle(UpdateVendorPartCommand request, CancellationToken ct)
    {
        var vp = await db.VendorParts
            .Include(x => x.PriceTiers)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"VendorPart {request.Id} not found");

        var body = request.Body;

        // Per-field patch — null means "leave alone". Trimming on string
        // fields mirrors CreateVendorPartHandler.
        if (body.VendorPartNumber is not null) vp.VendorPartNumber = body.VendorPartNumber.Trim();
        if (body.ManufacturerName is not null)
        {
            var trimmed = body.ManufacturerName.Trim();
            vp.ManufacturerName = trimmed.Length == 0 ? null : trimmed;
        }
        if (body.VendorMpn is not null) vp.VendorMpn = body.VendorMpn.Trim();
        if (body.LeadTimeDays.HasValue) vp.LeadTimeDays = body.LeadTimeDays.Value;
        if (body.MinOrderQty.HasValue) vp.MinOrderQty = body.MinOrderQty.Value;
        if (body.PackSize.HasValue) vp.PackSize = body.PackSize.Value;
        if (body.CountryOfOrigin is not null) vp.CountryOfOrigin = body.CountryOfOrigin.Trim().ToUpperInvariant();
        if (body.HtsCode is not null) vp.HtsCode = body.HtsCode.Trim();
        if (body.Certifications is not null) vp.Certifications = body.Certifications;
        if (body.LastQuotedDate.HasValue) vp.LastQuotedDate = body.LastQuotedDate.Value;
        if (body.Notes is not null) vp.Notes = body.Notes;

        vp.IsApproved = body.IsApproved;

        // Preferred-flip guard — only when transitioning false → true do we
        // need to clear siblings; otherwise leave the rest of the AVL alone.
        if (body.IsPreferred && !vp.IsPreferred)
        {
            var siblings = await db.VendorParts
                .Where(other => other.PartId == vp.PartId
                    && other.Id != vp.Id
                    && other.IsPreferred)
                .ToListAsync(ct);
            foreach (var sib in siblings)
                sib.IsPreferred = false;
        }
        vp.IsPreferred = body.IsPreferred;

        await db.SaveChangesAsync(ct);

        var loaded = await db.VendorParts
            .Include(x => x.Vendor)
            .Include(x => x.Part)
            .Include(x => x.PriceTiers)
            .AsNoTracking()
            .FirstAsync(x => x.Id == vp.Id, ct);

        return VendorPartMapper.ToResponse(loaded);
    }
}
