using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Jobs;

public class AccountingCacheSyncJob(
    IAccountingProviderFactory providerFactory,
    ISystemSettingRepository systemSettings,
    ILogger<AccountingCacheSyncJob> logger)
{
    public async Task RefreshCacheAsync(CancellationToken ct = default)
    {
        var accountingService = await providerFactory.GetActiveProviderAsync(ct);
        if (accountingService is null)
        {
            logger.LogInformation("No accounting provider configured — skipping cache refresh");
            return;
        }

        logger.LogInformation("Starting accounting cache refresh via {Provider}", accountingService.ProviderName);

        var syncStatus = await accountingService.GetSyncStatusAsync(ct);

        if (!syncStatus.Connected)
        {
            logger.LogInformation("Accounting provider not connected — skipping cache refresh");
            return;
        }

        var customers = await accountingService.GetCustomersAsync(ct);
        var count = customers.Count;
        var now = DateTimeOffset.UtcNow.ToString("O");

        await systemSettings.UpsertAsync(
            "accounting_last_sync",
            now,
            "Timestamp of the last successful accounting cache sync",
            ct);

        await systemSettings.UpsertAsync(
            "accounting_cached_customers",
            count.ToString(),
            "Number of customers retrieved from the accounting provider in the last cache sync",
            ct);

        logger.LogInformation("Accounting cache refreshed: {Count} customers", count);
    }
}
