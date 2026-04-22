namespace QBEngineer.Core.Models;

/// <summary>
/// Configuration for qb-engineer's optional OIDC identity provider surface.
/// Bound from the <c>Oidc</c> configuration section in appsettings.json and environment variables.
/// </summary>
public class OidcOptions
{
    public const string SectionName = "Oidc";

    /// <summary>
    /// Public base URL of the qb-engineer API (scheme + host + optional port, no trailing slash).
    /// Used to construct <c>registration_client_uri</c> and absolute endpoint URLs in discovery metadata.
    /// Example: <c>https://qbengineer.example.com</c>.
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// When false, the entire OIDC provider surface (including <c>/connect/*</c> endpoints and
    /// admin controllers) behaves as if disabled — registrations are rejected and authorization
    /// attempts return 404. Administrators flip this to true from the Integrations panel after
    /// reading the enablement guide.
    /// </summary>
    public bool ProviderEnabled { get; set; }
}
