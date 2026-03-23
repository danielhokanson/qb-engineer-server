using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record TrainingProgressResponseModel(
    int ModuleId,
    TrainingProgressStatus Status,
    int? QuizScore,
    int? QuizAttempts,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int TimeSpentSeconds
);
