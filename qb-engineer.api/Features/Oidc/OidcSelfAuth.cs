using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Shared authentication helper for RFC 7592 client self-management endpoints.
/// Validates that the presented registration_access_token hashes to the stored hash
/// for the given client_id AND that the client is not revoked. Throws
/// <see cref="OidcRegistrationException"/> with <c>invalid_token</c> on any failure — never
/// leaks which part of the lookup failed.
/// </summary>
internal static class OidcSelfAuth
{
    internal static async Task<OidcClientMetadata> AuthenticateAsync(
        AppDbContext db,
        string clientId,
        string rawRegistrationAccessToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawRegistrationAccessToken))
        {
            throw new OidcRegistrationException(
                OidcRegistrationException.Errors.InvalidToken,
                "A registration_access_token bearer credential is required.");
        }

        var metadata = await db.OidcClientMetadata
            .FirstOrDefaultAsync(m => m.ClientId == clientId, ct);

        var presentedHash = OidcCrypto.HashSha256(rawRegistrationAccessToken);

        // Constant-time comparison path: compute even when metadata is null to resist timing attacks.
        var storedHash = metadata?.RegistrationAccessTokenHash ?? string.Empty;
        var match = storedHash.Length > 0 && OidcCrypto.ConstantTimeEquals(storedHash, presentedHash);

        if (metadata is null || !match || metadata.Status == OidcClientStatus.Revoked)
        {
            throw new OidcRegistrationException(
                OidcRegistrationException.Errors.InvalidToken,
                "The presented registration_access_token is not valid for this client.");
        }

        return metadata;
    }
}
