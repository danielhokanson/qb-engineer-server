namespace QBEngineer.Core.Models;

public record BarcodeResponseModel(
    int Id,
    string Value,
    string EntityType,
    bool IsActive,
    DateTime CreatedAt);
