namespace QBEngineer.Core.Models;

public record UpdateUserPreferencesRequestModel(
    Dictionary<string, object?> Preferences);
