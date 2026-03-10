namespace QBEngineer.Core.Models;

public record QuickBooksConnectionStatus(
    bool IsConnected,
    string? CompanyId,
    string? CompanyName,
    DateTime? ConnectedAt,
    DateTime? TokenExpiresAt,
    DateTime? LastSyncAt);
