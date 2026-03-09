namespace QBEngineer.Core.Models;

public record ReferenceDataGroupResponseModel(string GroupCode, List<ReferenceDataResponseModel> Values);

public record ReferenceDataResponseModel(
    int Id,
    string Code,
    string Label,
    int SortOrder,
    bool IsActive,
    string? Metadata);

public record CreateReferenceDataRequestModel(
    string GroupCode,
    string Code,
    string Label,
    int SortOrder,
    string? Metadata);

public record UpdateReferenceDataRequestModel(
    string? Label,
    int? SortOrder,
    bool? IsActive,
    string? Metadata);
