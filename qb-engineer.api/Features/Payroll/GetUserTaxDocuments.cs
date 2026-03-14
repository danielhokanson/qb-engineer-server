using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Payroll;

public record GetUserTaxDocumentsQuery(int UserId) : IRequest<List<TaxDocumentResponseModel>>;

public class GetUserTaxDocumentsHandler(AppDbContext db)
    : IRequestHandler<GetUserTaxDocumentsQuery, List<TaxDocumentResponseModel>>
{
    public async Task<List<TaxDocumentResponseModel>> Handle(
        GetUserTaxDocumentsQuery request, CancellationToken ct)
    {
        var docs = await db.TaxDocuments
            .AsNoTracking()
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.TaxYear)
            .ThenBy(d => d.DocumentType)
            .ToListAsync(ct);

        return docs.Select(d => new TaxDocumentResponseModel(
            d.Id, d.UserId, d.DocumentType, d.TaxYear,
            d.EmployerName, d.FileAttachmentId, d.Source, d.ExternalId
        )).ToList();
    }
}
