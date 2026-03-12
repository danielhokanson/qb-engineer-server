namespace QBEngineer.Core.Entities;

public class KioskTerminal : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public int ConfiguredByUserId { get; set; }
    public bool IsActive { get; set; } = true;

    public Team Team { get; set; } = null!;
}
