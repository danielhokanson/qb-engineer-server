using Microsoft.Extensions.Logging;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class MockAccountingService : IAccountingService
{
    private readonly ILogger<MockAccountingService> _logger;

    private static readonly List<AccountingCustomer> MockCustomers =
    [
        new("MOCK-CUST-001", "Acme Corp", "acme@example.com", "555-0101", "Acme Corporation", 2500.00m),
        new("MOCK-CUST-002", "Quantum Dynamics", "info@quantum.com", "555-0102", "Quantum Dynamics LLC", 0.00m),
        new("MOCK-CUST-003", "Apex Manufacturing", "sales@apex.com", "555-0103", "Apex Manufacturing Inc", 750.00m),
        new("MOCK-CUST-004", "Meridian Systems", "contact@meridian.com", "555-0104", "Meridian Systems Corp", 0.00m),
    ];

    public MockAccountingService(ILogger<MockAccountingService> logger)
    {
        _logger = logger;
    }

    public Task<List<AccountingCustomer>> GetCustomersAsync(CancellationToken ct)
    {
        _logger.LogInformation("[MockAccounting] GetCustomers — returning {Count} customers", MockCustomers.Count);
        return Task.FromResult(MockCustomers);
    }

    public Task<AccountingCustomer?> GetCustomerAsync(string externalId, CancellationToken ct)
    {
        var customer = MockCustomers.FirstOrDefault(c => c.ExternalId == externalId);
        _logger.LogInformation("[MockAccounting] GetCustomer({ExternalId}) — {Result}", externalId, customer != null ? "found" : "not found");
        return Task.FromResult(customer);
    }

    public Task<string> CreateCustomerAsync(AccountingCustomer customer, CancellationToken ct)
    {
        var id = $"MOCK-CUST-{Guid.NewGuid().ToString("N")[..8]}";
        _logger.LogInformation("[MockAccounting] CreateCustomer({Name}) — assigned {ExternalId}", customer.Name, id);
        return Task.FromResult(id);
    }

    public Task<string> CreateEstimateAsync(AccountingDocument document, CancellationToken ct)
    {
        var id = $"MOCK-EST-{Guid.NewGuid().ToString("N")[..8]}";
        _logger.LogInformation("[MockAccounting] CreateEstimate for {Customer}, {Amount:C} — assigned {Id}", document.CustomerExternalId, document.Amount, id);
        return Task.FromResult(id);
    }

    public Task<string> CreateInvoiceAsync(AccountingDocument document, CancellationToken ct)
    {
        var id = $"MOCK-INV-{Guid.NewGuid().ToString("N")[..8]}";
        _logger.LogInformation("[MockAccounting] CreateInvoice for {Customer}, {Amount:C} — assigned {Id}", document.CustomerExternalId, document.Amount, id);
        return Task.FromResult(id);
    }

    public Task<string> CreatePurchaseOrderAsync(AccountingDocument document, CancellationToken ct)
    {
        var id = $"MOCK-PO-{Guid.NewGuid().ToString("N")[..8]}";
        _logger.LogInformation("[MockAccounting] CreatePurchaseOrder, {Amount:C} — assigned {Id}", document.Amount, id);
        return Task.FromResult(id);
    }

    public Task<AccountingPayment?> GetPaymentAsync(string externalId, CancellationToken ct)
    {
        _logger.LogInformation("[MockAccounting] GetPayment({ExternalId}) — returning mock payment", externalId);
        var payment = new AccountingPayment(externalId, 1000.00m, DateTime.UtcNow.AddDays(-3), "Check");
        return Task.FromResult<AccountingPayment?>(payment);
    }

    public Task<string> CreateTimeActivityAsync(AccountingTimeActivity activity, CancellationToken ct)
    {
        var id = $"MOCK-TIME-{Guid.NewGuid().ToString("N")[..8]}";
        _logger.LogInformation("[MockAccounting] CreateTimeActivity for {Employee}, {Hours}h — assigned {Id}", activity.EmployeeExternalId, activity.Hours, id);
        return Task.FromResult(id);
    }

    public Task<bool> TestConnectionAsync(CancellationToken ct)
    {
        _logger.LogInformation("[MockAccounting] TestConnection — returning true");
        return Task.FromResult(true);
    }

    public Task<AccountingSyncStatus> GetSyncStatusAsync(CancellationToken ct)
    {
        _logger.LogInformation("[MockAccounting] GetSyncStatus");
        var status = new AccountingSyncStatus(true, DateTime.UtcNow, 0, 0);
        return Task.FromResult(status);
    }
}
