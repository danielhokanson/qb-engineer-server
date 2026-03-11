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
    string? ExternalPartNumber,
    int BomEntryCount,
    DateTime CreatedAt);
