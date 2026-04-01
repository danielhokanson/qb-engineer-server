using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class IdentityDocument : BaseAuditableEntity
{
    public int UserId { get; set; }
    public IdentityDocumentType DocumentType { get; set; }
    public int FileAttachmentId { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public int? VerifiedById { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? Notes { get; set; }

    public FileAttachment FileAttachment { get; set; } = null!;
}
