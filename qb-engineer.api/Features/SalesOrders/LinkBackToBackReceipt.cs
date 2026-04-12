using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.SalesOrders;

public record LinkBackToBackReceiptCommand(int PurchaseOrderId, int PurchaseOrderLineId, LinkBackToBackReceiptRequestModel Request) : IRequest;

public class LinkBackToBackReceiptHandler(AppDbContext db, IBackToBackService backToBackService) : IRequestHandler<LinkBackToBackReceiptCommand>
{
    public async Task Handle(LinkBackToBackReceiptCommand command, CancellationToken cancellationToken)
    {
        var poLine = await db.PurchaseOrderLines
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == command.PurchaseOrderLineId && l.PurchaseOrderId == command.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order line {command.PurchaseOrderLineId} not found");

        await backToBackService.LinkReceiptToSalesOrderAsync(command.PurchaseOrderLineId, command.Request.ReceivingRecordId, cancellationToken);
    }
}
