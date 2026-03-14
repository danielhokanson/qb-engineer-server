using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class ComplianceFormSubmission : BaseAuditableEntity
{
    public int TemplateId { get; set; }
    public int UserId { get; set; }
    public int? DocuSealSubmissionId { get; set; }
    public ComplianceSubmissionStatus Status { get; set; } = ComplianceSubmissionStatus.Pending;
    public DateTime? SignedAt { get; set; }
    public int? SignedPdfFileId { get; set; }
    public string? DocuSealSubmitUrl { get; set; }

    public ComplianceFormTemplate Template { get; set; } = null!;
    public FileAttachment? SignedPdfFile { get; set; }
}
