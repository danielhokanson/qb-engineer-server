namespace QBEngineer.Core.Models;

public record UpdateFormDefinitionRequestModel(
    string FormDefinitionJson,
    string? Revision);
