namespace QBEngineer.Api.Features.ReferenceData;

public record ReferenceDataGroupDto(string GroupCode, List<ReferenceDataDto> Values);

public record ReferenceDataDto(
    int Id,
    string Code,
    string Label,
    int SortOrder,
    bool IsActive,
    string? Metadata);
