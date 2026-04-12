namespace QBEngineer.Core.Models;

public record TranslationEntryResponseModel(
    string Key,
    string Value,
    string? Context,
    bool IsApproved);
