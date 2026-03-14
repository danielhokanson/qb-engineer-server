using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UploadTaxDocumentRequestModel(
    TaxDocumentType DocumentType,
    int TaxYear,
    int FileAttachmentId);
