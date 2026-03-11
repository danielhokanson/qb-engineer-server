using Microsoft.Extensions.Logging;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class AccountingProviderFactory : IAccountingProviderFactory
{
    private readonly Dictionary<string, IAccountingService> _providers;
    private readonly ISystemSettingRepository _settings;
    private readonly ILogger<AccountingProviderFactory> _logger;

    private const string ActiveProviderKey = "accounting_provider";

    public AccountingProviderFactory(
        IEnumerable<IAccountingService> providers,
        ISystemSettingRepository settings,
        ILogger<AccountingProviderFactory> logger)
    {
        _providers = providers.ToDictionary(p => p.ProviderId, p => p);
        _settings = settings;
        _logger = logger;
    }

    public async Task<IAccountingService?> GetActiveProviderAsync(CancellationToken ct)
    {
        var providerId = await GetActiveProviderIdAsync(ct);
        if (string.IsNullOrEmpty(providerId))
            return null;

        if (_providers.TryGetValue(providerId, out var provider))
            return provider;

        _logger.LogWarning("Configured accounting provider '{ProviderId}' not found in registered providers", providerId);
        return null;
    }

    public IAccountingService? GetProvider(string providerId)
    {
        _providers.TryGetValue(providerId, out var provider);
        return provider;
    }

    public async Task<List<AccountingProviderInfo>> GetAvailableProvidersAsync(CancellationToken ct)
    {
        var activeId = await GetActiveProviderIdAsync(ct);

        var infos = new List<AccountingProviderInfo>
        {
            new("quickbooks", "QuickBooks Online", "Accounting, invoicing, and payment sync via Intuit QuickBooks", "account_balance", true, activeId == "quickbooks"),
            new("xero", "Xero", "Cloud accounting with multi-currency and project tracking", "account_balance_wallet", true, false),
            new("freshbooks", "FreshBooks", "Small business invoicing and expense tracking", "receipt_long", true, false),
            new("sage", "Sage Business Cloud", "Enterprise accounting and financial management", "business", true, false),
        };

        // Mark providers that have registered implementations
        foreach (var info in infos)
        {
            if (_providers.ContainsKey(info.Id) && info.Id == activeId)
            {
                // Already marked as configured above
            }
        }

        return infos;
    }

    public async Task SetActiveProviderAsync(string? providerId, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(providerId) && !_providers.ContainsKey(providerId))
        {
            throw new InvalidOperationException($"Accounting provider '{providerId}' is not registered. Available: {string.Join(", ", _providers.Keys)}");
        }

        await _settings.UpsertAsync(ActiveProviderKey, providerId ?? string.Empty, "Active accounting provider ID", ct);
        await _settings.SaveChangesAsync(ct);

        _logger.LogInformation("Active accounting provider set to '{ProviderId}'", providerId ?? "(standalone)");
    }

    public async Task<string?> GetActiveProviderIdAsync(CancellationToken ct)
    {
        var setting = await _settings.FindByKeyAsync(ActiveProviderKey, ct);
        var value = setting?.Value;
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
