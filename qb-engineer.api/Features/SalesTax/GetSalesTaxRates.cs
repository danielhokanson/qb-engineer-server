using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.SalesTax;

public record GetSalesTaxRatesQuery : IRequest<List<SalesTaxRateResponseModel>>;

public class GetSalesTaxRatesHandler(AppDbContext db) : IRequestHandler<GetSalesTaxRatesQuery, List<SalesTaxRateResponseModel>>
{
    public async Task<List<SalesTaxRateResponseModel>> Handle(GetSalesTaxRatesQuery request, CancellationToken cancellationToken)
    {
        return await db.SalesTaxRates
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new SalesTaxRateResponseModel(
                r.Id, r.Name, r.Code, r.Rate, r.IsDefault, r.IsActive, r.Description))
            .ToListAsync(cancellationToken);
    }
}
