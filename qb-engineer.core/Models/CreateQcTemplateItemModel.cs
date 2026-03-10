namespace QBEngineer.Core.Models;

public record CreateQcTemplateItemModel(
    string Description,
    string? Specification,
    int SortOrder,
    bool IsRequired);
