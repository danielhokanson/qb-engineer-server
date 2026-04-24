using System.Text.Json.Serialization;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// RFC 7591 (Dynamic Client Registration) + RFC 7592 (Client Configuration Endpoint).
/// These endpoints are intentionally unauthenticated at the controller level — bearer credentials
/// are parsed manually from the <c>Authorization</c> header and validated inside the MediatR
/// handlers. RFC 7591 errors are returned as <c>{ "error", "error_description" }</c> JSON per spec.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("connect/register")]
public class OidcRegistrationController(IMediator mediator, IOidcProviderSettings providerSettings) : ControllerBase
{

    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    private static string? ReadBearer(HttpRequest req)
    {
        var auth = req.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(auth)) return null;
        const string prefix = "Bearer ";
        return auth.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? auth[prefix.Length..].Trim() : null;
    }

    /// <summary>
    /// RFC 7591 Dynamic Client Registration. The ticket is passed in the <c>Authorization</c>
    /// header as a Bearer credential. Client metadata is in the JSON body.
    /// Returns the newly registered client in <c>Pending</c> state — an admin must approve
    /// before the client can complete an authorization flow.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterClientRequest body)
    {
        var settings = await providerSettings.GetAsync(HttpContext.RequestAborted);
        if (!settings.ProviderEnabled)
        {
            return StatusCode(503, new RegistrationErrorResponse(
                "temporarily_unavailable",
                "The OIDC identity-provider surface is not enabled on this installation."));
        }

        var ticket = ReadBearer(Request);
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return Unauthorized(new RegistrationErrorResponse(
                OidcRegistrationException.Errors.InvalidToken,
                "A registration ticket is required in the Authorization: Bearer header."));
        }

        try
        {
            var result = await mediator.Send(new RedeemTicketCommand(
                RawTicket: ticket,
                ClientName: body.ClientName ?? string.Empty,
                RedirectUris: (IReadOnlyList<string>?)body.RedirectUris ?? Array.Empty<string>(),
                PostLogoutRedirectUris: body.PostLogoutRedirectUris,
                Scopes: ParseScopes(body.Scope),
                GrantTypes: body.GrantTypes,
                ResponseTypes: body.ResponseTypes,
                TokenEndpointAuthMethod: body.TokenEndpointAuthMethod,
                SoftwareStatement: body.SoftwareStatement,
                Contacts: body.Contacts,
                ClientUri: body.ClientUri,
                LogoUri: body.LogoUri,
                TosUri: body.TosUri,
                PolicyUri: body.PolicyUri,
                CallerIp: GetIp()));

            var registrationClientUri = BuildRegistrationClientUri(result.ClientId, settings.PublicBaseUrl);

            var response = new RegisterClientResponse
            {
                ClientId = result.ClientId,
                ClientSecret = result.ClientSecret,
                ClientIdIssuedAt = result.IssuedAt.ToUnixTimeSeconds(),
                ClientSecretExpiresAt = 0,
                RegistrationAccessToken = result.RegistrationAccessToken,
                RegistrationClientUri = registrationClientUri,
                ClientName = result.ClientName,
                RedirectUris = result.RedirectUris,
                PostLogoutRedirectUris = result.PostLogoutRedirectUris,
                GrantTypes = result.GrantTypes,
                ResponseTypes = result.ResponseTypes,
                Scope = string.Join(' ', result.Scopes),
                TokenEndpointAuthMethod = result.TokenEndpointAuthMethod,
            };

            Response.Headers.Location = registrationClientUri;
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (OidcRegistrationException ex)
        {
            var status = ex.Error == OidcRegistrationException.Errors.InvalidToken
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status400BadRequest;
            return StatusCode(status, new RegistrationErrorResponse(ex.Error, ex.Description));
        }
    }

    /// <summary>
    /// RFC 7592 client self-read. Client authenticates with the registration_access_token.
    /// On every successful call the token is rotated; the new token is returned exactly once.
    /// </summary>
    [HttpGet("{clientId}")]
    public async Task<IActionResult> ReadSelf(string clientId)
    {
        var settings = await providerSettings.GetAsync(HttpContext.RequestAborted);
        if (!settings.ProviderEnabled)
        {
            return StatusCode(503);
        }

        var token = ReadBearer(Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new RegistrationErrorResponse(
                OidcRegistrationException.Errors.InvalidToken,
                "A registration_access_token is required in the Authorization: Bearer header."));
        }

        try
        {
            var self = await mediator.Send(new GetSelfClientQuery(clientId, token, GetIp()));
            var registrationClientUri = BuildRegistrationClientUri(clientId, settings.PublicBaseUrl);
            return Ok(new SelfClientConfigurationResponse
            {
                ClientId = self.ClientId,
                ClientName = self.ClientName,
                RedirectUris = self.RedirectUris,
                PostLogoutRedirectUris = self.PostLogoutRedirectUris,
                GrantTypes = self.GrantTypes,
                ResponseTypes = self.ResponseTypes,
                Scope = string.Join(' ', self.Scopes),
                TokenEndpointAuthMethod = self.TokenEndpointAuthMethod,
                RegistrationAccessToken = self.NewRegistrationAccessToken,
                RegistrationClientUri = registrationClientUri,
                Status = self.Status.ToString(),
                RequireConsent = self.RequireConsent,
                IsFirstParty = self.IsFirstParty,
            });
        }
        catch (OidcRegistrationException ex)
        {
            return StatusCode(StatusCodes.Status401Unauthorized,
                new RegistrationErrorResponse(ex.Error, ex.Description));
        }
    }

    /// <summary>
    /// RFC 7592 client self-deletion. Client authenticates with the registration_access_token
    /// and revokes itself. Idempotent — repeated deletions after the token is cleared will 401.
    /// </summary>
    [HttpDelete("{clientId}")]
    public async Task<IActionResult> DeleteSelf(string clientId)
    {
        var settings = await providerSettings.GetAsync(HttpContext.RequestAborted);
        if (!settings.ProviderEnabled)
        {
            return StatusCode(503);
        }

        var token = ReadBearer(Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new RegistrationErrorResponse(
                OidcRegistrationException.Errors.InvalidToken,
                "A registration_access_token is required in the Authorization: Bearer header."));
        }

        try
        {
            await mediator.Send(new DeleteSelfClientCommand(clientId, token, GetIp()));
            return NoContent();
        }
        catch (OidcRegistrationException ex)
        {
            return StatusCode(StatusCodes.Status401Unauthorized,
                new RegistrationErrorResponse(ex.Error, ex.Description));
        }
    }

    private string BuildRegistrationClientUri(string clientId, string? configuredBaseUrl)
    {
        var baseUrl = configuredBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var req = Request;
            baseUrl = $"{req.Scheme}://{req.Host}";
        }
        return $"{baseUrl.TrimEnd('/')}/connect/register/{clientId}";
    }

    private static IReadOnlyList<string> ParseScopes(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope)) return Array.Empty<string>();
        return scope
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}

/// <summary>
/// RFC 7591 §2 client metadata payload. All field names intentionally snake_case per spec.
/// </summary>
public class RegisterClientRequest
{
    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    [JsonPropertyName("redirect_uris")]
    public List<string>? RedirectUris { get; set; }

    [JsonPropertyName("post_logout_redirect_uris")]
    public List<string>? PostLogoutRedirectUris { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("grant_types")]
    public List<string>? GrantTypes { get; set; }

    [JsonPropertyName("response_types")]
    public List<string>? ResponseTypes { get; set; }

    [JsonPropertyName("token_endpoint_auth_method")]
    public string? TokenEndpointAuthMethod { get; set; }

    [JsonPropertyName("software_statement")]
    public string? SoftwareStatement { get; set; }

    [JsonPropertyName("contacts")]
    public List<string>? Contacts { get; set; }

    [JsonPropertyName("client_uri")]
    public string? ClientUri { get; set; }

    [JsonPropertyName("logo_uri")]
    public string? LogoUri { get; set; }

    [JsonPropertyName("tos_uri")]
    public string? TosUri { get; set; }

    [JsonPropertyName("policy_uri")]
    public string? PolicyUri { get; set; }
}

/// <summary>
/// RFC 7591 §3.2.1 successful registration response.
/// </summary>
public class RegisterClientResponse
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("client_id_issued_at")]
    public long ClientIdIssuedAt { get; set; }

    [JsonPropertyName("client_secret_expires_at")]
    public long ClientSecretExpiresAt { get; set; }

    [JsonPropertyName("registration_access_token")]
    public string RegistrationAccessToken { get; set; } = string.Empty;

    [JsonPropertyName("registration_client_uri")]
    public string RegistrationClientUri { get; set; } = string.Empty;

    [JsonPropertyName("client_name")]
    public string ClientName { get; set; } = string.Empty;

    [JsonPropertyName("redirect_uris")]
    public IReadOnlyList<string> RedirectUris { get; set; } = Array.Empty<string>();

    [JsonPropertyName("post_logout_redirect_uris")]
    public IReadOnlyList<string> PostLogoutRedirectUris { get; set; } = Array.Empty<string>();

    [JsonPropertyName("grant_types")]
    public IReadOnlyList<string> GrantTypes { get; set; } = Array.Empty<string>();

    [JsonPropertyName("response_types")]
    public IReadOnlyList<string> ResponseTypes { get; set; } = Array.Empty<string>();

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; } = string.Empty;
}

/// <summary>
/// RFC 7592 §2.1 client configuration response. Same envelope as the registration response
/// plus qb-engineer-specific lifecycle fields.
/// </summary>
public class SelfClientConfigurationResponse
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    [JsonPropertyName("redirect_uris")]
    public IReadOnlyList<string> RedirectUris { get; set; } = Array.Empty<string>();

    [JsonPropertyName("post_logout_redirect_uris")]
    public IReadOnlyList<string> PostLogoutRedirectUris { get; set; } = Array.Empty<string>();

    [JsonPropertyName("grant_types")]
    public IReadOnlyList<string> GrantTypes { get; set; } = Array.Empty<string>();

    [JsonPropertyName("response_types")]
    public IReadOnlyList<string> ResponseTypes { get; set; } = Array.Empty<string>();

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; } = string.Empty;

    [JsonPropertyName("registration_access_token")]
    public string RegistrationAccessToken { get; set; } = string.Empty;

    [JsonPropertyName("registration_client_uri")]
    public string RegistrationClientUri { get; set; } = string.Empty;

    [JsonPropertyName("qb_engineer_status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("qb_engineer_require_consent")]
    public bool RequireConsent { get; set; }

    [JsonPropertyName("qb_engineer_is_first_party")]
    public bool IsFirstParty { get; set; }
}

/// <summary>RFC 7591 §3.2.2 error envelope.</summary>
public record RegistrationErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string ErrorDescription);
