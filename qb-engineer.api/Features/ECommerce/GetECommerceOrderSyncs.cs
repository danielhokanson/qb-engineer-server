using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ECommerce;

public record GetECommerceOrderSyncsQuery(int IntegrationId) : IRequest<List<ECommerceOrderSyncResponseModel>>;

public class GetECommerceOrderSyncsHandler(AppDbContext db)
    : IRequestHandler<GetECommerceOrderSyncsQuery, List<ECommerceOrderSyncResponseModel>>
{
    public async Task<List<ECommerceOrderSyncResponseModel>> Handle(
        GetECommerceOrderSyncsQuery request, CancellationToken cancellationToken)
    {
        var syncs = await db.ECommerceOrderSyncs
            .AsNoTracking()
            .Where(s => s.IntegrationId == request.IntegrationId)
            .OrderByDescending(s => s.ImportedAt)
            .Take(500)
            .ToListAsync(cancellationToken);

        return syncs.Select(s => new ECommerceOrderSyncResponseModel
        {
            Id = s.Id,
            ExternalOrderId = s.ExternalOrderId,
            ExternalOrderNumber = s.ExternalOrderNumber,
            SalesOrderId = s.SalesOrderId,
            Status = s.Status,
            ErrorMessage = s.ErrorMessage,
            ImportedAt = s.ImportedAt,
        }).ToList();
    }
}
