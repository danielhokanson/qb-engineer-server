using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/quickbooks")]
public class QuickBooksController(
    IOptions<QuickBooksOptions> options,
    IQuickBooksTokenService tokenService,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<QuickBooksController> logger) : ControllerBase
{
    /// <summary>
    /// Initiates the QuickBooks OAuth 2.0 authorization flow.
    /// Returns the Intuit authorization URL for the client to redirect to.
    /// </summary>
    [HttpGet("authorize")]
    [Authorize(Roles = "Admin")]
    public IActionResult Authorize()
    {
        var opts = options.Value;
        if (string.IsNullOrEmpty(opts.ClientId))
            return BadRequest("QuickBooks integration is not configured");

        var state = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("qb_oauth_state", state);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["client_id"] = opts.ClientId;
        query["scope"] = opts.Scopes;
        query["redirect_uri"] = opts.RedirectUri;
        query["response_type"] = "code";
        query["state"] = state;

        var authUrl = $"{opts.AuthorizationEndpoint}?{query}";
        return Ok(new { authorizationUrl = authUrl });
    }

    /// <summary>
    /// OAuth callback handler. Exchanges the authorization code for access/refresh tokens.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        [FromQuery(Name = "realmId")] string realmId,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(realmId))
            return BadRequest("Missing authorization code or realm ID");

        var opts = options.Value;
        var client = httpClientFactory.CreateClient();

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opts.ClientId}:{opts.ClientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = opts.RedirectUri,
        });

        var response = await client.PostAsync(opts.TokenEndpoint, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("QuickBooks token exchange failed: {StatusCode} {Body}", response.StatusCode, body);
            return BadRequest("Failed to exchange authorization code for tokens");
        }

        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        var tokenData = new QuickBooksTokenData(
            AccessToken: root.GetProperty("access_token").GetString()!,
            RefreshToken: root.GetProperty("refresh_token").GetString()!,
            RealmId: realmId,
            AccessTokenExpiresAt: DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32()),
            RefreshTokenExpiresAt: DateTime.UtcNow.AddSeconds(root.GetProperty("x_refresh_token_expires_in").GetInt32()));

        await tokenService.SaveTokenAsync(tokenData, ct);

        logger.LogInformation("QuickBooks connected successfully. RealmId: {RealmId}", realmId);

        // Redirect to the Angular admin page after successful OAuth
        var frontendUrl = configuration["FrontendBaseUrl"] ?? "http://localhost:4200";
        return Redirect($"{frontendUrl}/admin/integrations?qb=connected");
    }

    /// <summary>
    /// Returns the current QuickBooks connection status.
    /// </summary>
    [HttpGet("status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var token = await tokenService.GetTokenAsync(ct);
        if (token is null || token.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            return Ok(new QuickBooksConnectionStatus(
                IsConnected: false,
                CompanyId: null,
                CompanyName: null,
                ConnectedAt: null,
                TokenExpiresAt: null,
                LastSyncAt: null));
        }

        // Fetch company name from QB API
        string? companyName = null;
        try
        {
            var accessToken = await tokenService.GetValidAccessTokenAsync(ct);
            if (accessToken is not null)
            {
                companyName = await FetchCompanyNameAsync(accessToken, token.RealmId, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch QuickBooks company name");
        }

        return Ok(new QuickBooksConnectionStatus(
            IsConnected: true,
            CompanyId: token.RealmId,
            CompanyName: companyName,
            ConnectedAt: null, // Could track this in SystemSettings if needed
            TokenExpiresAt: token.AccessTokenExpiresAt,
            LastSyncAt: null));
    }

    /// <summary>
    /// Disconnects from QuickBooks by revoking tokens.
    /// </summary>
    [HttpPost("disconnect")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disconnect(CancellationToken ct)
    {
        await tokenService.ClearTokenAsync(ct);
        logger.LogInformation("QuickBooks disconnected");
        return NoContent();
    }

    /// <summary>
    /// Tests the QuickBooks connection by querying the company info endpoint.
    /// </summary>
    [HttpPost("test")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestConnection(CancellationToken ct)
    {
        var accessToken = await tokenService.GetValidAccessTokenAsync(ct);
        if (accessToken is null)
            return Ok(new { success = false, message = "Not connected or token expired" });

        var token = await tokenService.GetTokenAsync(ct);
        if (token is null)
            return Ok(new { success = false, message = "No token data found" });

        try
        {
            var companyName = await FetchCompanyNameAsync(accessToken, token.RealmId, ct);
            return Ok(new { success = true, companyName });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuickBooks connection test failed");
            return Ok(new { success = false, message = ex.Message });
        }
    }

    private async Task<string?> FetchCompanyNameAsync(string accessToken, string realmId, CancellationToken ct)
    {
        var opts = options.Value;
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var url = $"{opts.BaseApiUrl}/v3/company/{realmId}/companyinfo/{realmId}";
        var response = await client.GetAsync(url, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to fetch company info: {StatusCode}", response.StatusCode);
            return null;
        }

        var json = JsonDocument.Parse(body);
        return json.RootElement
            .GetProperty("CompanyInfo")
            .GetProperty("CompanyName")
            .GetString();
    }
}
