using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IQuickBooksTokenService
{
    Task<QuickBooksTokenData?> GetTokenAsync(CancellationToken ct);
    Task SaveTokenAsync(QuickBooksTokenData tokenData, CancellationToken ct);
    Task<string?> GetValidAccessTokenAsync(CancellationToken ct);
    Task ClearTokenAsync(CancellationToken ct);
    Task<bool> IsConnectedAsync(CancellationToken ct);
}
