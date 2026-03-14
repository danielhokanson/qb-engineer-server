namespace QBEngineer.Core.Models;

public record ProfileCompletenessResponseModel(
    bool IsComplete,
    bool CanBeAssignedJobs,
    int TotalItems,
    int CompletedItems,
    List<ProfileCompletenessItem> Items);

public record ProfileCompletenessItem(
    string Key,
    string Label,
    bool IsComplete,
    bool BlocksJobAssignment);
