using FluentAssertions;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Handlers.Oidc;

public class MintTicketHandlerTests
{
    private readonly AppDbContext _db = TestDbContextFactory.Create();
    private readonly FakeOidcAuditService _audit = new();
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 4, 21, 12, 0, 0, TimeSpan.Zero));

    private MintTicketHandler BuildHandler() => new(_db, _clock, _audit);

    private MintTicketCommand BuildCommand(
        string? redirectPrefix = null,
        IReadOnlyList<string>? scopes = null,
        int? ttl = null,
        bool requireSigned = false,
        IReadOnlyList<string>? trustedKids = null) => new(
            ExpectedClientName: "Reporting Portal",
            AllowedRedirectUriPrefix: redirectPrefix ?? "https://reporting.example.com/",
            AllowedPostLogoutRedirectUriPrefix: null,
            AllowedScopes: scopes ?? new[] { "openid", "profile", "qb.parts.read" },
            RequiredRolesForUsers: new[] { "Admin" },
            TtlHours: ttl ?? 24,
            RequireSignedSoftwareStatement: requireSigned,
            TrustedPublisherKeyIds: trustedKids,
            Notes: "test ticket",
            IssuedByUserId: 1,
            IssuedFromIp: "127.0.0.1");

    [Fact]
    public async Task Handle_Valid_CreatesTicketAndReturnsRawValueOnce()
    {
        var result = await BuildHandler().Handle(BuildCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.RawTicket.Should().StartWith("oidt_");
        result.TicketPrefix.Should().HaveLength(8);
        result.RawTicket.Should().StartWith(result.TicketPrefix);
        result.ExpiresAt.Should().Be(_clock.UtcNow.AddHours(24));
        result.AllowedScopes.Should().Contain("qb.parts.read");
    }

    [Fact]
    public async Task Handle_Persists_OnlyHashNeverRawValue()
    {
        var result = await BuildHandler().Handle(BuildCommand(), CancellationToken.None);

        var stored = _db.OidcRegistrationTickets.Single();
        stored.TicketHash.Should().Be(OidcCrypto.HashSha256(result.RawTicket));
        stored.TicketHash.Should().NotContain(result.RawTicket);
        stored.Status.Should().Be(OidcTicketStatus.Issued);
        stored.IssuedByUserId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_SerializesAllowedScopes_AsCsv()
    {
        await BuildHandler().Handle(BuildCommand(scopes: new[] { "openid", "profile", "custom.x" }), CancellationToken.None);

        _db.OidcRegistrationTickets.Single().AllowedScopesCsv
            .Should().Be("openid,profile,custom.x");
    }

    [Fact]
    public async Task Handle_RecordsAuditEvent_TicketIssued()
    {
        await BuildHandler().Handle(BuildCommand(), CancellationToken.None);

        _audit.Events.Should().ContainSingle();
        _audit.Events[0].EventType.Should().Be(OidcAuditEventType.TicketIssued);
        _audit.Events[0].ActorUserId.Should().Be(1);
        _audit.Events[0].ActorIp.Should().Be("127.0.0.1");
        _audit.Events[0].TicketId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_TtlInHours_SetsExpiresAt()
    {
        var result = await BuildHandler().Handle(BuildCommand(ttl: 72), CancellationToken.None);

        result.ExpiresAt.Should().Be(_clock.UtcNow.AddHours(72));
    }

    [Fact]
    public async Task Handle_WithoutTrustedKids_SignedStatementNotRequired_StoresEmpty()
    {
        await BuildHandler().Handle(BuildCommand(requireSigned: false), CancellationToken.None);

        var stored = _db.OidcRegistrationTickets.Single();
        stored.RequireSignedSoftwareStatement.Should().BeFalse();
        stored.TrustedPublisherKeyIdsCsv.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithSignedStatementRequired_PersistsTrustedKids()
    {
        await BuildHandler().Handle(
            BuildCommand(requireSigned: true, trustedKids: new[] { "kid1", "kid2" }),
            CancellationToken.None);

        var stored = _db.OidcRegistrationTickets.Single();
        stored.RequireSignedSoftwareStatement.Should().BeTrue();
        stored.TrustedPublisherKeyIdsCsv.Should().Be("kid1,kid2");
    }

    [Fact]
    public void Validator_HttpsOrLocalhost_AllowedOnly()
    {
        var validator = new MintTicketValidator();

        validator.Validate(BuildCommand(redirectPrefix: "http://evil.example.com/")).IsValid.Should().BeFalse();
        validator.Validate(BuildCommand(redirectPrefix: "https://good.example.com/")).IsValid.Should().BeTrue();
        validator.Validate(BuildCommand(redirectPrefix: "http://localhost:3000/")).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_EmptyScopes_Invalid()
    {
        var validator = new MintTicketValidator();
        validator.Validate(BuildCommand(scopes: Array.Empty<string>())).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_TtlOutsideRange_Invalid()
    {
        var validator = new MintTicketValidator();
        validator.Validate(BuildCommand(ttl: 0)).IsValid.Should().BeFalse();
        validator.Validate(BuildCommand(ttl: 200)).IsValid.Should().BeFalse();
        validator.Validate(BuildCommand(ttl: 24)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_SignedStatementWithoutKids_Invalid()
    {
        var validator = new MintTicketValidator();
        validator.Validate(BuildCommand(requireSigned: true, trustedKids: null)).IsValid.Should().BeFalse();
        validator.Validate(BuildCommand(requireSigned: true, trustedKids: new[] { "kid1" })).IsValid.Should().BeTrue();
    }
}
