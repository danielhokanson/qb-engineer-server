using FluentValidation;
using MediatR;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

public record MintTicketCommand(
    string ExpectedClientName,
    string AllowedRedirectUriPrefix,
    string? AllowedPostLogoutRedirectUriPrefix,
    IReadOnlyList<string> AllowedScopes,
    IReadOnlyList<string>? RequiredRolesForUsers,
    int TtlHours,
    bool RequireSignedSoftwareStatement,
    IReadOnlyList<string>? TrustedPublisherKeyIds,
    string? Notes,
    int IssuedByUserId,
    string? IssuedFromIp) : IRequest<MintTicketResponse>;

/// <summary>
/// Admin response. <see cref="RawTicket"/> is returned exactly once and never retrievable again.
/// </summary>
public record MintTicketResponse(
    int TicketId,
    string RawTicket,
    string TicketPrefix,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    string AllowedRedirectUriPrefix,
    string[] AllowedScopes);

public class MintTicketValidator : AbstractValidator<MintTicketCommand>
{
    public MintTicketValidator()
    {
        RuleFor(x => x.ExpectedClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AllowedRedirectUriPrefix)
            .NotEmpty()
            .MaximumLength(500)
            .Must(p => p.StartsWith("https://", StringComparison.Ordinal)
                      || p.StartsWith("http://localhost", StringComparison.Ordinal))
            .WithMessage("Redirect URI prefix must be https:// or http://localhost for local dev.");
        RuleFor(x => x.AllowedScopes).NotEmpty().WithMessage("At least one scope must be allowed.");
        RuleFor(x => x.TtlHours).InclusiveBetween(1, 168);
        RuleFor(x => x.TrustedPublisherKeyIds)
            .NotEmpty()
            .When(x => x.RequireSignedSoftwareStatement)
            .WithMessage("Trusted publisher key IDs required when signed software statement is required.");
    }
}

public class MintTicketHandler(AppDbContext db, IClock clock, IOidcAuditService audit)
    : IRequestHandler<MintTicketCommand, MintTicketResponse>
{
    public async Task<MintTicketResponse> Handle(MintTicketCommand request, CancellationToken ct)
    {
        var raw = OidcCrypto.GenerateTicket();
        var hash = OidcCrypto.HashSha256(raw);
        var now = clock.UtcNow;

        var ticket = new OidcRegistrationTicket
        {
            TicketPrefix = raw[..Math.Min(8, raw.Length)],
            TicketHash = hash,
            IssuedByUserId = request.IssuedByUserId,
            IssuedAt = now,
            ExpiresAt = now.AddHours(request.TtlHours),
            AllowedRedirectUriPrefix = request.AllowedRedirectUriPrefix,
            AllowedPostLogoutRedirectUriPrefix = request.AllowedPostLogoutRedirectUriPrefix,
            AllowedScopesCsv = string.Join(",", request.AllowedScopes),
            RequiredRolesForUsersCsv = request.RequiredRolesForUsers is { Count: > 0 }
                ? string.Join(",", request.RequiredRolesForUsers)
                : null,
            RequireSignedSoftwareStatement = request.RequireSignedSoftwareStatement,
            TrustedPublisherKeyIdsCsv = request.TrustedPublisherKeyIds is { Count: > 0 }
                ? string.Join(",", request.TrustedPublisherKeyIds)
                : null,
            ExpectedClientName = request.ExpectedClientName,
            Notes = request.Notes,
            Status = OidcTicketStatus.Issued,
        };

        db.OidcRegistrationTickets.Add(ticket);
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.TicketIssued,
            actorUserId: request.IssuedByUserId,
            actorIp: request.IssuedFromIp,
            ticketId: ticket.Id,
            details: new
            {
                ticket.ExpectedClientName,
                ticket.AllowedRedirectUriPrefix,
                AllowedScopes = request.AllowedScopes.ToArray(),
                request.TtlHours,
                request.RequireSignedSoftwareStatement,
            },
            ct: ct);

        return new MintTicketResponse(
            ticket.Id,
            raw,
            ticket.TicketPrefix,
            ticket.IssuedAt,
            ticket.ExpiresAt,
            ticket.AllowedRedirectUriPrefix,
            request.AllowedScopes.ToArray());
    }
}
