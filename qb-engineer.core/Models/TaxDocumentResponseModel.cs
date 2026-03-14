using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record TaxDocumentResponseModel(
    int Id,
    int UserId,
    TaxDocumentType DocumentType,
    int TaxYear,
    string? EmployerName,
    int? FileAttachmentId,
    PayrollDocumentSource Source,
    string? ExternalId);
