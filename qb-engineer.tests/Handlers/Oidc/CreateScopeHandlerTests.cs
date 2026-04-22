using FluentAssertions;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Handlers.Oidc;

public class CreateScopeHandlerTests
{
    private readonly AppDbContext _db = TestDbContextFactory.Create();
    private readonly FakeOidcAuditService _audit = new();

    private CreateScopeHandler BuildHandler() => new(_db, _audit);

    private static CreateScopeCommand BuildCommand(
        string name = "qb.parts.read",
        string claimMappings = """[{"claimType":"qb_permissions","source":"role","value":"Admin"}]""") =>
        new(name, $"Display {name}", $"Description for {name}", claimMappings,
            ResourcesCsv: null, ActorUserId: 1, ActorIp: "127.0.0.1");

    [Fact]
    public async Task Handle_Valid_PersistsScopeAndReturnsId()
    {
        var id = await BuildHandler().Handle(BuildCommand(), CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var stored = _db.OidcCustomScopes.Single();
        stored.Name.Should().Be("qb.parts.read");
        stored.IsActive.Should().BeTrue();
        stored.IsSystem.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Duplicate_ThrowsInvalidOperation()
    {
        _db.OidcCustomScopes.Add(new OidcCustomScope
        {
            Name = "qb.parts.read",
            DisplayName = "Existing",
            Description = "Existing scope",
            ClaimMappingsJson = "[]",
        });
        await _db.SaveChangesAsync();

        var act = () => BuildHandler().Handle(BuildCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*'qb.parts.read'*already exists*");
    }

    [Fact]
    public async Task Handle_RecordsAuditEvent_ScopeCreated()
    {
        await BuildHandler().Handle(BuildCommand(), CancellationToken.None);

        _audit.Events.Should().ContainSingle();
        _audit.Events[0].EventType.Should().Be(OidcAuditEventType.ScopeCreated);
        _audit.Events[0].ScopeName.Should().Be("qb.parts.read");
        _audit.Events[0].ActorUserId.Should().Be(1);
    }

    [Theory]
    [InlineData("qb.parts.read", true)]
    [InlineData("qb_permissions", true)]
    [InlineData("qb:admin", true)]
    [InlineData("invalid space", false)]
    [InlineData("bad/slash", false)]
    [InlineData("", false)]
    public void Validator_NameCharacterRules_EnforcesRegex(string name, bool expected)
    {
        var validator = new CreateScopeValidator();
        var result = validator.Validate(BuildCommand(name: name));
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("[]", true)]
    [InlineData("""[{"claimType":"x","source":"role","value":"Admin"}]""", true)]
    [InlineData("""{"claimType":"x"}""", false)]
    [InlineData("not-json", false)]
    [InlineData("", false)]
    public void Validator_ClaimMappingsJsonMustBeArray(string json, bool expected)
    {
        var validator = new CreateScopeValidator();
        var result = validator.Validate(BuildCommand(claimMappings: json));
        result.IsValid.Should().Be(expected);
    }
}
