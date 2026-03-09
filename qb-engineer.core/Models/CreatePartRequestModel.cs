using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreatePartRequestModel(
    string PartNumber,
    string Description,
    string? Revision,
    PartType PartType,
    string? Material,
    string? MoldToolRef);
