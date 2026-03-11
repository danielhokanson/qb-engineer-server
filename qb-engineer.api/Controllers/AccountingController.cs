using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class AccountingController(
    IAccountingProviderFactory providerFactory,
    IQuickBooksTokenService tokenService,
    IOptions<QuickBooksOptions> qbOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<AccountingController> logger) : ControllerBase
{
    // ─── Accounting Mode ───

    [AllowAnonymous]
    [HttpGet("admin/accounting-mode")]
    public async Task<ActionResult<AccountingModeResponse>> GetAccountingMode(CancellationToken ct)
    {
        var providerId = await providerFactory.GetActiveProviderIdAsync(ct);
        var provider = await providerFactory.GetActiveProviderAsync(ct);

        return Ok(new AccountingModeResponse(
            IsConfigured: provider is not null,
            ProviderName: provider?.ProviderName,
            ProviderId: providerId));
    }

    [HttpPut("admin/accounting-mode")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetAccountingMode([FromBody] SetAccountingModeRequest request, CancellationToken ct)
    {
        await providerFactory.SetActiveProviderAsync(request.ProviderId, ct);
        return NoContent();
    }

    // ─── Provider Registry ───

    [HttpGet("accounting/providers")]
    public async Task<ActionResult<List<AccountingProviderInfo>>> GetProviders(CancellationToken ct)
    {
        var providers = await providerFactory.GetAvailableProvidersAsync(ct);
        return Ok(providers);
    }

    // ─── Connection Status ───

    [HttpGet("accounting/status")]
    public async Task<ActionResult<AccountingConnectionStatusResponse>> GetStatus(CancellationToken ct)
    {
        var provider = await providerFactory.GetActiveProviderAsync(ct);
        if (provider is null)
        {
            return Ok(new AccountingConnectionStatusResponse(
                IsConnected: false,
                ProviderId: null,
                ProviderName: null,
                SyncStatus: null));
        }

        var syncStatus = await provider.GetSyncStatusAsync(ct);
        return Ok(new AccountingConnectionStatusResponse(
            IsConnected: syncStatus.Connected,
            ProviderId: provider.ProviderId,
            ProviderName: provider.ProviderName,
            SyncStatus: syncStatus));
    }

    [HttpPost("accounting/test")]
    public async Task<ActionResult> TestConnection(CancellationToken ct)
    {
        var provider = await providerFactory.GetActiveProviderAsync(ct);
        if (provider is null)
            return Ok(new { success = false, message = "No accounting provider configured" });

        var success = await provider.TestConnectionAsync(ct);
        return Ok(new { success, providerName = provider.ProviderName, message = success ? "Connection verified" : "Connection failed" });
    }

    [HttpPost("accounting/disconnect")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disconnect(CancellationToken ct)
    {
        var providerId = await providerFactory.GetActiveProviderIdAsync(ct);
        if (providerId == "quickbooks")
        {
            await tokenService.ClearTokenAsync(ct);
        }

        await providerFactory.SetActiveProviderAsync(null, ct);
        return NoContent();
    }

    // ─── QuickBooks OAuth ───

    [HttpGet("quickbooks/authorize")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAuthorizationUrl()
    {
        var opts = qbOptions.Value;
        if (string.IsNullOrEmpty(opts.ClientId))
            return BadRequest(new { message = "QuickBooks is not configured. Set ClientId and ClientSecret in appsettings." });

        var state = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("qb_oauth_state", state);

        var authUrl = $"{opts.AuthorizationEndpoint}" +
            $"?client_id={Uri.EscapeDataString(opts.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(opts.RedirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(opts.Scopes)}" +
            $"&state={state}";

        return Ok(new { authorizationUrl = authUrl });
    }

    [HttpGet("quickbooks/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery] string realmId,
        CancellationToken ct)
    {
        var savedState = HttpContext.Session.GetString("qb_oauth_state");
        if (savedState is null || savedState != state)
        {
            logger.LogWarning("[QuickBooks] OAuth state mismatch");
            return BadRequest("Invalid OAuth state");
        }

        var opts = qbOptions.Value;
        var tokenUrl = opts.TokenEndpoint;

        using var client = httpClientFactory.CreateClient();
        var authHeader = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{opts.ClientId}:{opts.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = opts.RedirectUri,
        });

        var response = await client.PostAsync(tokenUrl, tokenRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("[QuickBooks] Token exchange failed: {StatusCode} {Body}", response.StatusCode, body);
            return BadRequest("Token exchange failed");
        }

        var tokenDoc = System.Text.Json.JsonDocument.Parse(body);
        var root = tokenDoc.RootElement;

        var tokenData = new QuickBooksTokenData(
            AccessToken: root.GetProperty("access_token").GetString()!,
            RefreshToken: root.GetProperty("refresh_token").GetString()!,
            RealmId: realmId,
            AccessTokenExpiresAt: DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32()),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddSeconds(root.GetProperty("x_refresh_token_expires_in").GetInt32()));

        await tokenService.SaveTokenAsync(tokenData, ct);
        await providerFactory.SetActiveProviderAsync("quickbooks", ct);

        logger.LogInformation("[QuickBooks] OAuth completed — connected to realm {RealmId}", realmId);

        // Redirect back to admin integrations page
        return Redirect("/admin?tab=integrations&qb=connected");
    }

    [HttpGet("quickbooks/status")]
    public async Task<ActionResult<QuickBooksConnectionStatus>> GetQuickBooksStatus(CancellationToken ct)
    {
        var isConnected = await tokenService.IsConnectedAsync(ct);
        var token = await tokenService.GetTokenAsync(ct);

        return Ok(new QuickBooksConnectionStatus(
            IsConnected: isConnected,
            CompanyId: token?.RealmId,
            CompanyName: null,
            ConnectedAt: null,
            TokenExpiresAt: token?.AccessTokenExpiresAt,
            LastSyncAt: null));
    }

    // ─── Employees (from accounting) ───

    [HttpGet("accounting/employees")]
    public async Task<ActionResult<List<AccountingEmployee>>> GetEmployees(CancellationToken ct)
    {
        var provider = await providerFactory.GetActiveProviderAsync(ct);
        if (provider is null)
            return Ok(new List<AccountingEmployee>());

        var employees = await provider.GetEmployeesAsync(ct);
        return Ok(employees);
    }

    // ─── Items (for linkage) ───

    [HttpGet("accounting/items")]
    public async Task<ActionResult<List<AccountingItem>>> GetItems(CancellationToken ct)
    {
        var provider = await providerFactory.GetActiveProviderAsync(ct);
        if (provider is null)
            return Ok(new List<AccountingItem>());

        var items = await provider.GetItemsAsync(ct);
        return Ok(items);
    }

    // ─── Sync Status ───

    [HttpGet("accounting/sync-status")]
    public async Task<ActionResult<AccountingSyncStatus>> GetSyncStatus(CancellationToken ct)
    {
        var provider = await providerFactory.GetActiveProviderAsync(ct);
        if (provider is null)
            return Ok(new AccountingSyncStatus(false, null, 0, 0));

        var status = await provider.GetSyncStatusAsync(ct);
        return Ok(status);
    }
}

public record SetAccountingModeRequest(string? ProviderId);

public record AccountingConnectionStatusResponse(
    bool IsConnected,
    string? ProviderId,
    string? ProviderName,
    AccountingSyncStatus? SyncStatus);
