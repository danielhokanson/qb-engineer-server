using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class IdentityDocument : BaseAuditableEntity
{
    public int UserId { get; set; }
    public IdentityDocumentType DocumentType { get; set; }
    public int FileAttachmentId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedById { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Notes { get; set; }

    public FileAttachment FileAttachment { get; set; } = null!;
}
