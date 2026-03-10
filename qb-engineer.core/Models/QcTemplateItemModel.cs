namespace QBEngineer.Core.Models;

public record QcTemplateItemModel(
    int Id,
    string Description,
    string? Specification,
    int SortOrder,
    bool IsRequired);
