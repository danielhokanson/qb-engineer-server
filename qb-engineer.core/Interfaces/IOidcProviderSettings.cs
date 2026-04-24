namespace QBEngineer.Core.Interfaces;

/// <summary>
/// Runtime-resolved OIDC provider config. Values live in <c>system_settings</c> (overridable
/// from the admin UI) with appsettings.json as the seeded fallback. Reading is cheap enough to
/// hit every request — the underlying repository is scoped-DbContext-backed and the values are
/// tiny, but if that ever matters we can layer a short-TTL in-memory cache here.
/// </summary>
public interface IOidcProviderSettings
{
    /// <summary>
    /// Snapshots the current settings in a single lookup. Prefer this over two separate calls
    /// to keep a request consistent against concurrent edits.
    /// </summary>
    Task<OidcProviderSettingsSnapshot> GetAsync(CancellationToken ct = default);

    /// <summary>Update both fields atomically from the admin UI.</summary>
    Task UpdateAsync(bool providerEnabled, string publicBaseUrl, CancellationToken ct = default);

    /// <summary>Synchronous snapshot used from hot paths that cannot await — returns the last
    /// resolved values, falling back to appsettings on first call.</summary>
    OidcProviderSettingsSnapshot GetSnapshot();
}

public sealed record OidcProviderSettingsSnapshot(bool ProviderEnabled, string PublicBaseUrl)
{
    public static OidcProviderSettingsSnapshot Empty { get; } = new(false, string.Empty);
}

/// <summary>
/// Canonical <c>system_settings</c> keys used by the OIDC provider settings service.
/// </summary>
public static class OidcSettingKeys
{
    public const string ProviderEnabled = "oidc.provider_enabled";
    public const string PublicBaseUrl = "oidc.public_base_url";
}
