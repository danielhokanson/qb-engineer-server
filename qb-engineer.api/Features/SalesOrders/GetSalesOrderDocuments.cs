using Microsoft.EntityFrameworkCore;

using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.SalesOrders;

public record GetSalesOrderDocumentsQuery(int SalesOrderId) : IRequest<List<FileAttachmentResponseModel>>;

public class GetSalesOrderDocumentsHandler(AppDbContext db, IFileRepository fileRepo)
    : IRequestHandler<GetSalesOrderDocumentsQuery, List<FileAttachmentResponseModel>>
{
    public async Task<List<FileAttachmentResponseModel>> Handle(
        GetSalesOrderDocumentsQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.SalesOrders.AnyAsync(so => so.Id == request.SalesOrderId, cancellationToken);
        if (!exists)
            throw new KeyNotFoundException($"Sales order {request.SalesOrderId} not found");

        return await fileRepo.GetByEntityAsync("sales-orders", request.SalesOrderId, cancellationToken);
    }
}
