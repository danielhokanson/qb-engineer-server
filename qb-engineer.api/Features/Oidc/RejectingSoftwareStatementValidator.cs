using Microsoft.Extensions.Logging;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Default <see cref="ISoftwareStatementValidator"/>. Rejects every software statement so that
/// tickets which require a signed statement cannot be redeemed until an administrator wires up a
/// real JWKS-backed validator. This is the safe default — the alternative (accept without verify)
/// would defeat the purpose of the <c>RequireSignedSoftwareStatement</c> flag.
/// </summary>
public class RejectingSoftwareStatementValidator(ILogger<RejectingSoftwareStatementValidator> logger)
    : ISoftwareStatementValidator
{
    public Task<SoftwareStatementClaims?> ValidateAsync(
        string jwt,
        IReadOnlyCollection<string> trustedKeyIds,
        CancellationToken ct = default)
    {
        logger.LogWarning(
            "Software statement presented but no publisher JWKS is configured. Rejecting. " +
            "Trusted kids: {TrustedKeyIds}",
            string.Join(",", trustedKeyIds));
        return Task.FromResult<SoftwareStatementClaims?>(null);
    }
}
