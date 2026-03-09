namespace QBEngineer.Core.Models;

public record JobLinkResponseModel(
    int Id,
    int SourceJobId,
    int TargetJobId,
    string LinkType,
    int LinkedJobId,
    string LinkedJobNumber,
    string LinkedJobTitle,
    string LinkedJobStageName,
    string LinkedJobStageColor,
    DateTime CreatedAt);
