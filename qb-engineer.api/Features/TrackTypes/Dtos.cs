namespace QBEngineer.Api.Features.TrackTypes;

public record TrackTypeDto(
    int Id,
    string Name,
    string Code,
    string? Description,
    bool IsDefault,
    int SortOrder,
    List<StageDto> Stages);

public record StageDto(
    int Id,
    string Name,
    string Code,
    int SortOrder,
    string Color,
    int? WIPLimit,
    string? AccountingDocumentType,
    bool IsIrreversible);
