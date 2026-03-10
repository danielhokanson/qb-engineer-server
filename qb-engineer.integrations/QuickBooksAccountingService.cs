using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class QuickBooksAccountingService(
    IQuickBooksTokenService tokenService,
    IHttpClientFactory httpClientFactory,
    IOptions<QuickBooksOptions> options,
    ILogger<QuickBooksAccountingService> logger) : IAccountingService
{
    public async Task<List<AccountingCustomer>> GetCustomersAsync(CancellationToken ct)
    {
        var json = await QueryAsync("SELECT * FROM Customer WHERE Active = true MAXRESULTS 1000", ct);
        if (json is null) return [];

        var customers = new List<AccountingCustomer>();
        if (json.RootElement.TryGetProperty("QueryResponse", out var qr) &&
            qr.TryGetProperty("Customer", out var arr))
        {
            foreach (var c in arr.EnumerateArray())
            {
                customers.Add(MapCustomer(c));
            }
        }

        logger.LogInformation("[QuickBooks] GetCustomers — returned {Count} customers", customers.Count);
        return customers;
    }

    public async Task<AccountingCustomer?> GetCustomerAsync(string externalId, CancellationToken ct)
    {
        var json = await QueryAsync($"SELECT * FROM Customer WHERE Id = '{externalId}'", ct);
        if (json is null) return null;

        if (json.RootElement.TryGetProperty("QueryResponse", out var qr) &&
            qr.TryGetProperty("Customer", out var arr))
        {
            foreach (var c in arr.EnumerateArray())
            {
                return MapCustomer(c);
            }
        }

        return null;
    }

    public async Task<string> CreateCustomerAsync(AccountingCustomer customer, CancellationToken ct)
    {
        var payload = new
        {
            DisplayName = customer.Name,
            CompanyName = customer.CompanyName,
            PrimaryEmailAddr = customer.Email is not null ? new { Address = customer.Email } : null,
            PrimaryPhone = customer.Phone is not null ? new { FreeFormNumber = customer.Phone } : null,
        };

        var result = await PostEntityAsync("customer", payload, ct);
        var id = result?.RootElement.GetProperty("Customer").GetProperty("Id").GetString()
            ?? throw new InvalidOperationException("Failed to create QuickBooks customer");

        logger.LogInformation("[QuickBooks] CreateCustomer({Name}) — assigned {Id}", customer.Name, id);
        return id;
    }

    public async Task<string> CreateEstimateAsync(AccountingDocument document, CancellationToken ct)
    {
        var payload = BuildDocumentPayload(document);
        var result = await PostEntityAsync("estimate", payload, ct);
        var id = result?.RootElement.GetProperty("Estimate").GetProperty("Id").GetString()
            ?? throw new InvalidOperationException("Failed to create QuickBooks estimate");

        logger.LogInformation("[QuickBooks] CreateEstimate for {Customer}, {Amount:C} — assigned {Id}",
            document.CustomerExternalId, document.Amount, id);
        return id;
    }

    public async Task<string> CreateInvoiceAsync(AccountingDocument document, CancellationToken ct)
    {
        var payload = BuildDocumentPayload(document);
        var result = await PostEntityAsync("invoice", payload, ct);
        var id = result?.RootElement.GetProperty("Invoice").GetProperty("Id").GetString()
            ?? throw new InvalidOperationException("Failed to create QuickBooks invoice");

        logger.LogInformation("[QuickBooks] CreateInvoice for {Customer}, {Amount:C} — assigned {Id}",
            document.CustomerExternalId, document.Amount, id);
        return id;
    }

    public async Task<string> CreatePurchaseOrderAsync(AccountingDocument document, CancellationToken ct)
    {
        var payload = new
        {
            VendorRef = new { value = document.CustomerExternalId },
            Line = document.LineItems.Select((li, i) => new
            {
                Id = (i + 1).ToString(),
                DetailType = "ItemBasedExpenseLineDetail",
                Amount = li.Quantity * li.UnitPrice,
                ItemBasedExpenseLineDetail = new
                {
                    ItemRef = li.ItemExternalId is not null ? new { value = li.ItemExternalId } : null,
                    Qty = li.Quantity,
                    UnitPrice = li.UnitPrice,
                },
                Description = li.Description,
            }).ToArray(),
            DocNumber = document.RefNumber,
            TxnDate = document.Date.ToString("yyyy-MM-dd"),
        };

        var result = await PostEntityAsync("purchaseorder", payload, ct);
        var id = result?.RootElement.GetProperty("PurchaseOrder").GetProperty("Id").GetString()
            ?? throw new InvalidOperationException("Failed to create QuickBooks purchase order");

        logger.LogInformation("[QuickBooks] CreatePurchaseOrder, {Amount:C} — assigned {Id}", document.Amount, id);
        return id;
    }

    public async Task<AccountingPayment?> GetPaymentAsync(string externalId, CancellationToken ct)
    {
        var json = await QueryAsync($"SELECT * FROM Payment WHERE Id = '{externalId}'", ct);
        if (json is null) return null;

        if (json.RootElement.TryGetProperty("QueryResponse", out var qr) &&
            qr.TryGetProperty("Payment", out var arr))
        {
            foreach (var p in arr.EnumerateArray())
            {
                return new AccountingPayment(
                    ExternalId: p.GetProperty("Id").GetString()!,
                    Amount: p.GetProperty("TotalAmt").GetDecimal(),
                    Date: DateTime.Parse(p.GetProperty("TxnDate").GetString()!),
                    Method: p.TryGetProperty("PaymentMethodRef", out var pm)
                        ? pm.GetProperty("name").GetString() ?? "Unknown"
                        : "Unknown");
            }
        }

        return null;
    }

    public async Task<string> CreateTimeActivityAsync(AccountingTimeActivity activity, CancellationToken ct)
    {
        var payload = new
        {
            NameOf = "Employee",
            EmployeeRef = new { value = activity.EmployeeExternalId },
            CustomerRef = activity.CustomerExternalId is not null
                ? new { value = activity.CustomerExternalId }
                : null,
            Hours = (int)activity.Hours,
            Minutes = (int)((activity.Hours - (int)activity.Hours) * 60),
            TxnDate = activity.Date.ToString("yyyy-MM-dd"),
            Description = activity.Description,
            ItemRef = activity.ServiceItemExternalId is not null
                ? new { value = activity.ServiceItemExternalId }
                : null,
        };

        var result = await PostEntityAsync("timeactivity", payload, ct);
        var id = result?.RootElement.GetProperty("TimeActivity").GetProperty("Id").GetString()
            ?? throw new InvalidOperationException("Failed to create QuickBooks time activity");

        logger.LogInformation("[QuickBooks] CreateTimeActivity for {Employee}, {Hours}h — assigned {Id}",
            activity.EmployeeExternalId, activity.Hours, id);
        return id;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct)
    {
        try
        {
            var json = await GetEntityAsync("companyinfo", ct);
            return json is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AccountingSyncStatus> GetSyncStatusAsync(CancellationToken ct)
    {
        var isConnected = await tokenService.IsConnectedAsync(ct);
        return new AccountingSyncStatus(isConnected, isConnected ? DateTime.UtcNow : null, 0, 0);
    }

    // --- Private helpers ---

    private async Task<JsonDocument?> QueryAsync(string query, CancellationToken ct)
    {
        var (client, realmId) = await GetAuthenticatedClientAsync(ct);
        if (client is null) return null;

        var opts = options.Value;
        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"{opts.BaseApiUrl}/v3/company/{realmId}/query?query={encodedQuery}";

        var response = await client.GetAsync(url, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("[QuickBooks] Query failed: {StatusCode} {Body}", response.StatusCode, body);
            return null;
        }

        return JsonDocument.Parse(body);
    }

    private async Task<JsonDocument?> GetEntityAsync(string entityPath, CancellationToken ct)
    {
        var (client, realmId) = await GetAuthenticatedClientAsync(ct);
        if (client is null) return null;

        var opts = options.Value;
        var url = $"{opts.BaseApiUrl}/v3/company/{realmId}/{entityPath}/{realmId}";

        var response = await client.GetAsync(url, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("[QuickBooks] GET {Entity} failed: {StatusCode}", entityPath, response.StatusCode);
            return null;
        }

        return JsonDocument.Parse(body);
    }

    private async Task<JsonDocument?> PostEntityAsync(string entity, object payload, CancellationToken ct)
    {
        var (client, realmId) = await GetAuthenticatedClientAsync(ct);
        if (client is null) throw new InvalidOperationException("QuickBooks not connected");

        var opts = options.Value;
        var url = $"{opts.BaseApiUrl}/v3/company/{realmId}/{entity}";

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(url, jsonContent, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("[QuickBooks] POST {Entity} failed: {StatusCode} {Body}", entity, response.StatusCode, body);
            throw new InvalidOperationException($"QuickBooks API error: {response.StatusCode}");
        }

        return JsonDocument.Parse(body);
    }

    private async Task<(HttpClient? Client, string RealmId)> GetAuthenticatedClientAsync(CancellationToken ct)
    {
        var accessToken = await tokenService.GetValidAccessTokenAsync(ct);
        if (accessToken is null)
        {
            logger.LogWarning("[QuickBooks] No valid access token available");
            return (null, string.Empty);
        }

        var token = await tokenService.GetTokenAsync(ct);
        if (token is null) return (null, string.Empty);

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return (client, token.RealmId);
    }

    private static object BuildDocumentPayload(AccountingDocument document)
    {
        return new
        {
            CustomerRef = new { value = document.CustomerExternalId },
            Line = document.LineItems.Select((li, i) => new
            {
                Id = (i + 1).ToString(),
                DetailType = "SalesItemLineDetail",
                Amount = li.Quantity * li.UnitPrice,
                SalesItemLineDetail = new
                {
                    ItemRef = li.ItemExternalId is not null ? new { value = li.ItemExternalId } : null,
                    Qty = li.Quantity,
                    UnitPrice = li.UnitPrice,
                },
                Description = li.Description,
            }).ToArray(),
            DocNumber = document.RefNumber,
            TxnDate = document.Date.ToString("yyyy-MM-dd"),
        };
    }

    private static AccountingCustomer MapCustomer(JsonElement c)
    {
        return new AccountingCustomer(
            ExternalId: c.GetProperty("Id").GetString()!,
            Name: c.GetProperty("DisplayName").GetString()!,
            Email: c.TryGetProperty("PrimaryEmailAddr", out var email)
                ? email.GetProperty("Address").GetString()
                : null,
            Phone: c.TryGetProperty("PrimaryPhone", out var phone)
                ? phone.GetProperty("FreeFormNumber").GetString()
                : null,
            CompanyName: c.TryGetProperty("CompanyName", out var company)
                ? company.GetString()
                : null,
            Balance: c.TryGetProperty("Balance", out var balance)
                ? balance.GetDecimal()
                : 0m);
    }
}
