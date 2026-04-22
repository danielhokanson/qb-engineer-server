using FluentAssertions;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Handlers.Oidc;

public class ApproveClientHandlerTests
{
    private readonly AppDbContext _db = TestDbContextFactory.Create();
    private readonly FakeOidcAuditService _audit = new();
    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 4, 21, 12, 0, 0, TimeSpan.Zero));

    private ApproveClientHandler BuildHandler() => new(_db, _clock, _audit);

    private async Task<OidcClientMetadata> SeedClient(OidcClientStatus status = OidcClientStatus.Pending)
    {
        var client = new OidcClientMetadata
        {
            ClientId = "test-client-id",
            Status = status,
            RequireConsent = true,
            IsFirstParty = false,
        };
        _db.OidcClientMetadata.Add(client);
        await _db.SaveChangesAsync();
        return client;
    }

    [Fact]
    public async Task Handle_Pending_TransitionsToActiveAndSetsApprover()
    {
        await SeedClient();

        await BuildHandler().Handle(
            new ApproveClientCommand("test-client-id", ActorUserId: 5, ActorIp: "10.0.0.1",
                IsFirstParty: true, RequireConsent: false,
                AllowedCustomScopesCsv: "qb.parts.read",
                RequiredRolesCsv: "Admin,Engineer",
                Notes: "approved after security review"),
            CancellationToken.None);

        var stored = _db.OidcClientMetadata.Single();
        stored.Status.Should().Be(OidcClientStatus.Active);
        stored.ApprovedByUserId.Should().Be(5);
        stored.ApprovedAt.Should().Be(_clock.UtcNow);
        stored.IsFirstParty.Should().BeTrue();
        stored.RequireConsent.Should().BeFalse();
        stored.AllowedCustomScopesCsv.Should().Be("qb.parts.read");
        stored.RequiredRolesCsv.Should().Be("Admin,Engineer");
        stored.Notes.Should().Be("approved after security review");
    }

    [Fact]
    public async Task Handle_NonPendingClient_Throws()
    {
        await SeedClient(status: OidcClientStatus.Active);

        var act = () => BuildHandler().Handle(
            new ApproveClientCommand("test-client-id", 1, null, false, true, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Active*only Pending*");
    }

    [Fact]
    public async Task Handle_UnknownClientId_Throws()
    {
        var act = () => BuildHandler().Handle(
            new ApproveClientCommand("nonexistent", 1, null, false, true, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_RecordsAuditEvent_ClientApproved()
    {
        await SeedClient();

        await BuildHandler().Handle(
            new ApproveClientCommand("test-client-id", 5, "10.0.0.1", true, true, null, null, null),
            CancellationToken.None);

        _audit.Events.Should().ContainSingle();
        _audit.Events[0].EventType.Should().Be(OidcAuditEventType.ClientApproved);
        _audit.Events[0].ActorUserId.Should().Be(5);
        _audit.Events[0].ClientId.Should().Be("test-client-id");
    }

    [Fact]
    public async Task Handle_NullNotes_DoesNotOverwriteExisting()
    {
        var client = await SeedClient();
        client.Notes = "original";
        await _db.SaveChangesAsync();

        await BuildHandler().Handle(
            new ApproveClientCommand("test-client-id", 1, null, false, true, null, null, Notes: null),
            CancellationToken.None);

        _db.OidcClientMetadata.Single().Notes.Should().Be("original");
    }
}
