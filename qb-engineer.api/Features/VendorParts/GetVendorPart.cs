using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.VendorParts;

/// <summary>
/// Pillar 3 — Single VendorPart read with PriceTiers + denormalized vendor
/// company name + part number/name.
/// </summary>
public record GetVendorPartQuery(int Id) : IRequest<VendorPartResponseModel>;

public class GetVendorPartHandler(AppDbContext db)
    : IRequestHandler<GetVendorPartQuery, VendorPartResponseModel>
{
    public async Task<VendorPartResponseModel> Handle(GetVendorPartQuery request, CancellationToken ct)
    {
        var vp = await db.VendorParts
            .Include(x => x.Vendor)
            .Include(x => x.Part)
            .Include(x => x.PriceTiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"VendorPart {request.Id} not found");

        return VendorPartMapper.ToResponse(vp);
    }
}
