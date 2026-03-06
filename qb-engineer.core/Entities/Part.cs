using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Part : BaseAuditableEntity
{
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Revision { get; set; } = "A";
    public PartStatus Status { get; set; } = PartStatus.Active;
    public PartType PartType { get; set; } = PartType.Part;
    public string? Material { get; set; }
    public string? MoldToolRef { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    // Custom fields (JSONB)
    public string? CustomFieldValues { get; set; }

    public ICollection<BOMEntry> BOMEntries { get; set; } = [];
    public ICollection<BOMEntry> UsedInBOM { get; set; } = [];
}
