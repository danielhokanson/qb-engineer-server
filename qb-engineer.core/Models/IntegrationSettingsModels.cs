namespace QBEngineer.Core.Models;

public record IntegrationSettingField(
    string Key,
    string Label,
    string Value,
    bool IsSensitive,
    bool IsRequired,
    string InputType = "text"
);

public record IntegrationStatusModel(
    string Provider,
    string Name,
    string Description,
    string Icon,
    bool IsConfigured,
    List<IntegrationSettingField> Fields,
    string Category = "service"
);

public record UpdateIntegrationSettingsRequestModel(
    Dictionary<string, string> Settings
);

public record TestIntegrationResultModel(
    bool Success,
    string Message
);
