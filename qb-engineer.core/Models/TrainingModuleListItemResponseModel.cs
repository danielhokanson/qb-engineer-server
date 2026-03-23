using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record TrainingModuleListItemResponseModel(
    int Id,
    string Title,
    string Slug,
    string Summary,
    TrainingContentType ContentType,
    string? CoverImageUrl,
    int EstimatedMinutes,
    string[] Tags,
    bool IsPublished,
    bool IsOnboardingRequired,
    int SortOrder,
    TrainingProgressStatus? MyStatus,
    int? MyQuizScore,
    DateTime? MyCompletedAt
);
