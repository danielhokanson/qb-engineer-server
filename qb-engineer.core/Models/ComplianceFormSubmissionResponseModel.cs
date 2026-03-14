using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ComplianceFormSubmissionResponseModel(
    int Id,
    int TemplateId,
    string TemplateName,
    ComplianceFormType FormType,
    ComplianceSubmissionStatus Status,
    DateTime? SignedAt,
    int? SignedPdfFileId,
    string? DocuSealSubmitUrl,
    DateTime CreatedAt);
