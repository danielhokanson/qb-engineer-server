using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdateBOMEntryRequestModel(
    decimal? Quantity,
    string? ReferenceDesignator,
    BOMSourceType? SourceType,
    string? Notes);
