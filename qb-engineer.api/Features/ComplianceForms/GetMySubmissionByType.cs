using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record GetMySubmissionByTypeQuery(
    int UserId,
    ComplianceFormType FormType) : IRequest<ComplianceFormSubmissionResponseModel?>;

public class GetMySubmissionByTypeHandler(AppDbContext db)
    : IRequestHandler<GetMySubmissionByTypeQuery, ComplianceFormSubmissionResponseModel?>
{
    public async Task<ComplianceFormSubmissionResponseModel?> Handle(
        GetMySubmissionByTypeQuery request, CancellationToken ct)
    {
        var submission = await db.ComplianceFormSubmissions
            .AsNoTracking()
            .Include(s => s.Template)
            .Where(s => s.UserId == request.UserId && s.Template.FormType == request.FormType)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (submission is null)
            return null;

        return new ComplianceFormSubmissionResponseModel(
            submission.Id, submission.TemplateId, submission.Template.Name,
            submission.Template.FormType, submission.Status, submission.SignedAt,
            submission.SignedPdfFileId, submission.DocuSealSubmitUrl,
            submission.CreatedAt
        );
    }
}
