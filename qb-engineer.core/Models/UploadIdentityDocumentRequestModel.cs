using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UploadIdentityDocumentRequestModel(
    IdentityDocumentType DocumentType,
    DateTime? ExpiresAt);
