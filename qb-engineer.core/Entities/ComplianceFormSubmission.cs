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
    public string? FormDataJson { get; set; }

    /// <summary>
    /// Pins this submission to the exact form definition version it was filled against.
    /// Allows regeneration of the form with the original layout at any time.
    /// </summary>
    public int? FormDefinitionVersionId { get; set; }

    public ComplianceFormTemplate Template { get; set; } = null!;
    public FormDefinitionVersion? FormDefinitionVersion { get; set; }
    public FileAttachment? SignedPdfFile { get; set; }
}
