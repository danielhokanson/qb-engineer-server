namespace QBEngineer.Core.Models;

public record ImportTranslationsRequestModel(
    Dictionary<string, string> Translations);
