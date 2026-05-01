using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.VendorParts;

/// <summary>
/// Pillar 3 — Catalog of vendors that source a given Part. Powers the
/// part-detail "Sources" tab. Sort order is intentional:
///   1. IsPreferred DESC — the preferred vendor is always at the top.
///   2. IsApproved DESC — approved vendors next, unapproved last.
///   3. VendorCompanyName ASC — alphabetic within each group.
/// </summary>
public record ListVendorPartsByPartQuery(int PartId)
    : IRequest<List<VendorPartResponseModel>>;

public class ListVendorPartsByPartHandler(AppDbContext db)
    : IRequestHandler<ListVendorPartsByPartQuery, List<VendorPartResponseModel>>
{
    public async Task<List<VendorPartResponseModel>> Handle(ListVendorPartsByPartQuery request, CancellationToken ct)
    {
        var rows = await db.VendorParts
            .Include(x => x.Vendor)
            .Include(x => x.Part)
            .Include(x => x.PriceTiers)
            .Where(x => x.PartId == request.PartId)
            .OrderByDescending(x => x.IsPreferred)
            .ThenByDescending(x => x.IsApproved)
            .ThenBy(x => x.Vendor!.CompanyName)
            .AsNoTracking()
            .ToListAsync(ct);

        return rows.Select(VendorPartMapper.ToResponse).ToList();
    }
}
