using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Asset : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string? Location { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public string? PhotoFileId { get; set; }
    public decimal CurrentHours { get; set; }
    public string? Notes { get; set; }

    // Tooling-specific fields
    public bool IsCustomerOwned { get; set; }
    public int? CavityCount { get; set; }
    public int? ToolLifeExpectancy { get; set; }
    public int CurrentShotCount { get; set; }
    public int? SourceJobId { get; set; }
    public Job? SourceJob { get; set; }
    public int? SourcePartId { get; set; }
    public Part? SourcePart { get; set; }
}
