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
    string? FormDataJson,
    int? FormDefinitionVersionId,
    DateTime CreatedAt,
    int? FilledPdfFileId,
    DateTime? I9Section1SignedAt,
    DateTime? I9Section2SignedAt,
    int? I9EmployerUserId,
    string? I9DocumentListType,
    string? I9DocumentDataJson,
    DateTime? I9Section2OverdueAt,
    DateTime? I9ReverificationDueAt);
