using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Default implementation of <see cref="IOidcProviderSettings"/>. Reads
/// <c>system_settings</c>, falls back to <see cref="OidcOptions"/> from appsettings.json when
/// a row is missing. The most-recently-resolved values are cached for <see cref="GetSnapshot"/>
/// so OpenIddict's hot-path issuer/discovery code can read synchronously.
/// </summary>
public class OidcProviderSettingsService : IOidcProviderSettings
{
    private readonly AppDbContext _db;
    private readonly OidcOptions _fallback;

    // Static cache keeps the last snapshot across scoped instances. First resolver wins on a
    // cold app; subsequent updates via UpdateAsync refresh it. Read-heavy, write-rare.
    private static OidcProviderSettingsSnapshot _cached = OidcProviderSettingsSnapshot.Empty;
    private static bool _cachePrimed;

    public OidcProviderSettingsService(AppDbContext db, IOptions<OidcOptions> fallback)
    {
        _db = db;
        _fallback = fallback.Value;
    }

    public async Task<OidcProviderSettingsSnapshot> GetAsync(CancellationToken ct = default)
    {
        var rows = await _db.SystemSettings
            .Where(s => s.Key == OidcSettingKeys.ProviderEnabled || s.Key == OidcSettingKeys.PublicBaseUrl)
            .ToListAsync(ct);

        var enabledRow = rows.FirstOrDefault(r => r.Key == OidcSettingKeys.ProviderEnabled);
        var urlRow = rows.FirstOrDefault(r => r.Key == OidcSettingKeys.PublicBaseUrl);

        var enabled = enabledRow is null
            ? _fallback.ProviderEnabled
            : ParseBool(enabledRow.Value, _fallback.ProviderEnabled);
        var url = string.IsNullOrWhiteSpace(urlRow?.Value) ? _fallback.PublicBaseUrl : urlRow!.Value;

        var snap = new OidcProviderSettingsSnapshot(enabled, url ?? string.Empty);
        _cached = snap;
        _cachePrimed = true;
        return snap;
    }

    public OidcProviderSettingsSnapshot GetSnapshot()
    {
        if (_cachePrimed) return _cached;
        // Cold cache — use appsettings fallback. Next GetAsync() call will prime.
        return new OidcProviderSettingsSnapshot(_fallback.ProviderEnabled, _fallback.PublicBaseUrl ?? string.Empty);
    }

    public async Task UpdateAsync(bool providerEnabled, string publicBaseUrl, CancellationToken ct = default)
    {
        await UpsertAsync(OidcSettingKeys.ProviderEnabled, providerEnabled ? "true" : "false",
            "Whether the OIDC identity-provider surface is turned on. Managed from the Integrations admin panel.", ct);
        await UpsertAsync(OidcSettingKeys.PublicBaseUrl, publicBaseUrl ?? string.Empty,
            "Public issuer / base URL advertised in OIDC discovery metadata. Managed from the Integrations admin panel.", ct);
        await _db.SaveChangesAsync(ct);
        _cached = new OidcProviderSettingsSnapshot(providerEnabled, publicBaseUrl ?? string.Empty);
        _cachePrimed = true;
    }

    private async Task UpsertAsync(string key, string value, string description, CancellationToken ct)
    {
        var existing = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (existing is null)
        {
            _db.SystemSettings.Add(new SystemSetting { Key = key, Value = value, Description = description });
        }
        else
        {
            existing.Value = value;
            // Keep description in sync so admins reading the raw table see what the key is for.
            existing.Description = description;
        }
    }

    private static bool ParseBool(string value, bool fallback)
        => bool.TryParse(value, out var parsed) ? parsed : fallback;
}
