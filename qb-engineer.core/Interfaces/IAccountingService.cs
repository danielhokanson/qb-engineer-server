using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IAccountingService
{
    Task<List<AccountingCustomer>> GetCustomersAsync(CancellationToken ct);
    Task<AccountingCustomer?> GetCustomerAsync(string externalId, CancellationToken ct);
    Task<string> CreateCustomerAsync(AccountingCustomer customer, CancellationToken ct);

    Task<string> CreateEstimateAsync(AccountingDocument document, CancellationToken ct);
    Task<string> CreateInvoiceAsync(AccountingDocument document, CancellationToken ct);
    Task<string> CreatePurchaseOrderAsync(AccountingDocument document, CancellationToken ct);

    Task<AccountingPayment?> GetPaymentAsync(string externalId, CancellationToken ct);
    Task<string> CreateTimeActivityAsync(AccountingTimeActivity activity, CancellationToken ct);

    Task<bool> TestConnectionAsync(CancellationToken ct);
    Task<AccountingSyncStatus> GetSyncStatusAsync(CancellationToken ct);
}
