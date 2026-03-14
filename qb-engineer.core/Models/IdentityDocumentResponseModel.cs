using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record IdentityDocumentResponseModel(
    int Id,
    int UserId,
    IdentityDocumentType DocumentType,
    int FileAttachmentId,
    string FileName,
    DateTime? VerifiedAt,
    int? VerifiedById,
    string? VerifiedByName,
    DateTime? ExpiresAt,
    string? Notes,
    DateTime CreatedAt);
