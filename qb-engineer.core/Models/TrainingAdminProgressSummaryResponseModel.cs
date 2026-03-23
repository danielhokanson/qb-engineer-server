namespace QBEngineer.Core.Models;

public record TrainingAdminProgressSummaryResponseModel(
    int UserId,
    string DisplayName,
    string Role,
    int TotalEnrolled,
    int TotalCompleted,
    int TotalModulesAcrossAllPaths,
    double OverallCompletionPct,
    DateTime? LastActivityAt
);
