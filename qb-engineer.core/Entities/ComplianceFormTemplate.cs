using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class ComplianceFormTemplate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public ComplianceFormType FormType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string? Sha256Hash { get; set; }
    public bool IsAutoSync { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public bool RequiresIdentityDocs { get; set; }
    public int? DocuSealTemplateId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public int? ManualOverrideFileId { get; set; }
    public bool BlocksJobAssignment { get; set; }
    public string ProfileCompletionKey { get; set; } = string.Empty;

    public FileAttachment? ManualOverrideFile { get; set; }
    public ICollection<ComplianceFormSubmission> Submissions { get; set; } = [];
}
