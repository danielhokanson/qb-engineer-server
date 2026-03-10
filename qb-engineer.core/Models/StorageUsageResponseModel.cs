namespace QBEngineer.Core.Models;

public record StorageUsageResponseModel(
    string EntityType,
    int FileCount,
    long TotalSizeBytes);
