using System.Security.Claims;
using System.Text.Json;

using QBEngineer.Core.Entities;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Translates an <see cref="OidcCustomScope.ClaimMappingsJson"/> rule set into a set of
/// <see cref="Claim"/> emissions for the given user principal + roles.
///
/// Rule schema (per mapping):
/// <code>
/// { "claimType": "qb_permissions",
///   "source": "role" | "profile" | "static" | "expression",
///   "value": "...",          // source-specific selector
///   "emittedValue": "..." }  // optional override — defaults to the matched value
/// </code>
/// Unknown sources are ignored rather than throwing — custom scopes are admin-editable.
/// </summary>
public static class ClaimMapper
{
    public static IEnumerable<Claim> ApplyMappings(
        string claimMappingsJson,
        ClaimsPrincipal principal,
        IReadOnlyCollection<string> userRoles)
    {
        if (string.IsNullOrWhiteSpace(claimMappingsJson)) yield break;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(claimMappingsJson);
        }
        catch (JsonException)
        {
            yield break;
        }

        using var _ = doc;
        if (doc.RootElement.ValueKind != JsonValueKind.Array) yield break;

        foreach (var rule in doc.RootElement.EnumerateArray())
        {
            if (rule.ValueKind != JsonValueKind.Object) continue;
            var claimType = rule.TryGetProperty("claimType", out var ct) ? ct.GetString() : null;
            var source = rule.TryGetProperty("source", out var s) ? s.GetString() : null;
            var value = rule.TryGetProperty("value", out var v) ? v.GetString() : null;
            var emittedOverride = rule.TryGetProperty("emittedValue", out var e) ? e.GetString() : null;

            if (string.IsNullOrWhiteSpace(claimType) || string.IsNullOrWhiteSpace(source)) continue;

            switch (source)
            {
                case "role":
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    if (value == "*")
                    {
                        foreach (var role in userRoles) yield return new Claim(claimType, role);
                    }
                    else if (userRoles.Contains(value, StringComparer.Ordinal))
                    {
                        yield return new Claim(claimType, emittedOverride ?? value);
                    }
                    break;

                case "profile":
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    var profileClaim = principal.FindFirst(value);
                    if (profileClaim is not null)
                    {
                        yield return new Claim(claimType, emittedOverride ?? profileClaim.Value);
                    }
                    break;

                case "static":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return new Claim(claimType, emittedOverride ?? value);
                    }
                    break;

                // "expression" reserved for future (e.g., role-and-department) — ignored for now.
            }
        }
    }
}
