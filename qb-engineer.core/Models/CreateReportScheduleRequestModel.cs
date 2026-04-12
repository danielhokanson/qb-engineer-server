using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateReportScheduleRequestModel(
    int SavedReportId,
    string CronExpression,
    string RecipientEmailsJson,
    ReportExportFormat Format,
    string? SubjectTemplate);
