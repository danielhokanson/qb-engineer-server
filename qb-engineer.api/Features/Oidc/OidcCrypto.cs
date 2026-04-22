using System.Security.Cryptography;
using System.Text;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Constant-time hashing + secure random generation for OIDC ticket and client secret material.
/// All raw secrets are shown to the admin/client exactly once at mint — only hashes persist.
/// </summary>
public static class OidcCrypto
{
    /// <summary>
    /// Generates a URL-safe random ticket string of ~43 chars (256 bits of entropy).
    /// Prefix "oidt_" makes accidental pastes searchable in logs.
    /// </summary>
    public static string GenerateTicket() => "oidt_" + GenerateBase64Url(32);

    /// <summary>Generates a client secret (~43 chars) with prefix <c>oids_</c>.</summary>
    public static string GenerateClientSecret() => "oids_" + GenerateBase64Url(32);

    /// <summary>Generates a registration access token (~43 chars) with prefix <c>oidr_</c>.</summary>
    public static string GenerateRegistrationAccessToken() => "oidr_" + GenerateBase64Url(32);

    /// <summary>SHA-256 hex hash. Deterministic, case-insensitive comparison.</summary>
    public static string HashSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>Constant-time string comparison.</summary>
    public static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (var i = 0; i < a.Length; i++) result |= a[i] ^ b[i];
        return result == 0;
    }

    private static string GenerateBase64Url(int byteLength)
    {
        var buffer = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(buffer)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
