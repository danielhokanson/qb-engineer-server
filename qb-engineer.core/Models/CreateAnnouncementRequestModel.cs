using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateAnnouncementRequestModel(
    string Title,
    string Content,
    AnnouncementSeverity Severity,
    AnnouncementScope Scope,
    bool RequiresAcknowledgment,
    DateTimeOffset? ExpiresAt,
    int? DepartmentId,
    List<int>? TargetTeamIds,
    int? TemplateId);
