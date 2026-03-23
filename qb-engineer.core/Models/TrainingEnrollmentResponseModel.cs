namespace QBEngineer.Core.Models;

public record TrainingEnrollmentResponseModel(
    int Id,
    int PathId,
    string PathTitle,
    string PathIcon,
    int TotalModules,
    int CompletedModules,
    DateTime? EnrolledAt,
    DateTime? CompletedAt
);
