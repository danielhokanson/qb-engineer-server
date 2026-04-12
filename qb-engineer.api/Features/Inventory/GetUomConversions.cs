using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Inventory;

public record GetUomConversionsQuery(int? PartId = null) : IRequest<List<UomConversionResponseModel>>;

public class GetUomConversionsHandler(AppDbContext db)
    : IRequestHandler<GetUomConversionsQuery, List<UomConversionResponseModel>>
{
    public async Task<List<UomConversionResponseModel>> Handle(GetUomConversionsQuery request, CancellationToken ct)
    {
        var query = db.UomConversions.AsNoTracking().AsQueryable();

        if (request.PartId.HasValue)
            query = query.Where(c => c.PartId == null || c.PartId == request.PartId);
        else
            query = query.Where(c => c.PartId == null);

        return await query
            .OrderBy(c => c.FromUom.Code)
            .ThenBy(c => c.ToUom.Code)
            .Select(c => new UomConversionResponseModel(
                c.Id, c.FromUomId, c.FromUom.Code,
                c.ToUomId, c.ToUom.Code,
                c.ConversionFactor, c.PartId, c.IsReversible))
            .ToListAsync(ct);
    }
}
