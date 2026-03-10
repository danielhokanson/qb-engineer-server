using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Dashboard;

public record GetOpenOrdersSummaryQuery : IRequest<OpenOrdersSummaryModel>;

public class GetOpenOrdersSummaryHandler(AppDbContext db)
    : IRequestHandler<GetOpenOrdersSummaryQuery, OpenOrdersSummaryModel>
{
    private static readonly SalesOrderStatus[] OpenStatuses =
    [
        SalesOrderStatus.Confirmed,
        SalesOrderStatus.InProduction,
        SalesOrderStatus.PartiallyShipped,
    ];

    public async Task<OpenOrdersSummaryModel> Handle(GetOpenOrdersSummaryQuery request, CancellationToken ct)
    {
        var orders = await db.SalesOrders
            .Include(so => so.Lines)
            .Where(so => OpenStatuses.Contains(so.Status))
            .ToListAsync(ct);

        return new OpenOrdersSummaryModel(
            TotalOrders: orders.Count,
            ConfirmedCount: orders.Count(o => o.Status == SalesOrderStatus.Confirmed),
            InProductionCount: orders.Count(o => o.Status == SalesOrderStatus.InProduction),
            PartiallyShippedCount: orders.Count(o => o.Status == SalesOrderStatus.PartiallyShipped),
            TotalValue: orders.Sum(o => o.Lines.Sum(l => l.LineTotal)));
    }
}
