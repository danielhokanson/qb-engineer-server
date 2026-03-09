namespace QBEngineer.Core.Models;

public record TrackTypeResponseModel(
    int Id,
    string Name,
    string Code,
    string? Description,
    bool IsDefault,
    int SortOrder,
    List<StageResponseModel> Stages);

public record StageResponseModel(
    int Id,
    string Name,
    string Code,
    int SortOrder,
    string Color,
    int? WIPLimit,
    string? AccountingDocumentType,
    bool IsIrreversible);
