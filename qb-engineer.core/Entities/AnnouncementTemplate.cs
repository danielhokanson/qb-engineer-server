using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class AnnouncementTemplate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementSeverity DefaultSeverity { get; set; } = AnnouncementSeverity.Info;
    public AnnouncementScope DefaultScope { get; set; } = AnnouncementScope.CompanyWide;
    public bool DefaultRequiresAcknowledgment { get; set; }
}
