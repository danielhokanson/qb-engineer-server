using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreatePartRequestModel(
    string Description,
    string? Revision,
    PartType PartType,
    string? Material,
    string? MoldToolRef,
    string? ExternalPartNumber);
