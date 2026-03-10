namespace QBEngineer.Core.Entities;

public class FileAttachment : BaseAuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int UploadedById { get; set; }
    public string? DocumentType { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? PartRevisionId { get; set; }
    public string? RequiredRole { get; set; }

    public PartRevision? PartRevision { get; set; }
}
