using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PartListResponseModel(
    int Id,
    string PartNumber,
    string Description,
    string Revision,
    PartStatus Status,
    PartType PartType,
    string? Material,
    int BomEntryCount,
    DateTime CreatedAt);

public record PartDetailResponseModel(
    int Id,
    string PartNumber,
    string Description,
    string Revision,
    PartStatus Status,
    PartType PartType,
    string? Material,
    string? MoldToolRef,
    string? ExternalId,
    string? ExternalRef,
    string? Provider,
    List<BOMEntryResponseModel> BomEntries,
    List<BOMUsageResponseModel> UsedIn,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record BOMEntryResponseModel(
    int Id,
    int ChildPartId,
    string ChildPartNumber,
    string ChildDescription,
    decimal Quantity,
    string? ReferenceDesignator,
    int SortOrder,
    BOMSourceType SourceType,
    string? Notes);

public record BOMUsageResponseModel(
    int Id,
    int ParentPartId,
    string ParentPartNumber,
    string ParentDescription,
    decimal Quantity);

public record CreatePartRequestModel(
    string PartNumber,
    string Description,
    string? Revision,
    PartType PartType,
    string? Material,
    string? MoldToolRef);

public record UpdatePartRequestModel(
    string? Description,
    string? Revision,
    PartStatus? Status,
    PartType? PartType,
    string? Material,
    string? MoldToolRef);

public record CreateBOMEntryRequestModel(
    int ChildPartId,
    decimal Quantity,
    string? ReferenceDesignator,
    BOMSourceType SourceType,
    string? Notes);

public record UpdateBOMEntryRequestModel(
    decimal? Quantity,
    string? ReferenceDesignator,
    BOMSourceType? SourceType,
    string? Notes);
