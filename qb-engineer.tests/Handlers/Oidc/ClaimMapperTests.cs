using System.Security.Claims;

using FluentAssertions;

using QBEngineer.Api.Features.Oidc;

namespace QBEngineer.Tests.Handlers.Oidc;

public class ClaimMapperTests
{
    private static ClaimsPrincipal BuildPrincipal(params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), "test");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void ApplyMappings_EmptyJson_YieldsNothing()
    {
        var claims = ClaimMapper.ApplyMappings(string.Empty, BuildPrincipal(), new[] { "Admin" });
        claims.Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_MalformedJson_YieldsNothingDoesNotThrow()
    {
        var result = ClaimMapper.ApplyMappings("{not-json", BuildPrincipal(), Array.Empty<string>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_NonArrayJson_YieldsNothing()
    {
        var result = ClaimMapper.ApplyMappings("{\"claimType\":\"x\"}", BuildPrincipal(), Array.Empty<string>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_RoleSource_ExactMatch_EmitsClaim()
    {
        var json = """[{"claimType":"qb_permissions","source":"role","value":"Admin"}]""";
        var claims = ClaimMapper.ApplyMappings(json, BuildPrincipal(), new[] { "Admin", "Engineer" }).ToList();

        claims.Should().HaveCount(1);
        claims[0].Type.Should().Be("qb_permissions");
        claims[0].Value.Should().Be("Admin");
    }

    [Fact]
    public void ApplyMappings_RoleSource_NoMatch_EmitsNothing()
    {
        var json = """[{"claimType":"qb_permissions","source":"role","value":"Admin"}]""";
        var claims = ClaimMapper.ApplyMappings(json, BuildPrincipal(), new[] { "Engineer" });

        claims.Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_RoleSource_Wildcard_EmitsAllRoles()
    {
        var json = """[{"claimType":"qb_role","source":"role","value":"*"}]""";
        var claims = ClaimMapper.ApplyMappings(json, BuildPrincipal(), new[] { "Admin", "Engineer", "Viewer" }).ToList();

        claims.Should().HaveCount(3);
        claims.Select(c => c.Value).Should().BeEquivalentTo("Admin", "Engineer", "Viewer");
    }

    [Fact]
    public void ApplyMappings_RoleSource_EmittedValueOverride_UsesOverride()
    {
        var json = """[{"claimType":"qb_permissions","source":"role","value":"Admin","emittedValue":"full_access"}]""";
        var claims = ClaimMapper.ApplyMappings(json, BuildPrincipal(), new[] { "Admin" }).ToList();

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be("full_access");
    }

    [Fact]
    public void ApplyMappings_ProfileSource_ClaimExists_EmitsValue()
    {
        var json = """[{"claimType":"qb_company_id","source":"profile","value":"company_id"}]""";
        var principal = BuildPrincipal(("company_id", "acme-42"));
        var claims = ClaimMapper.ApplyMappings(json, principal, Array.Empty<string>()).ToList();

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be("acme-42");
    }

    [Fact]
    public void ApplyMappings_ProfileSource_ClaimMissing_EmitsNothing()
    {
        var json = """[{"claimType":"qb_company_id","source":"profile","value":"company_id"}]""";
        var claims = ClaimMapper.ApplyMappings(json, BuildPrincipal(), Array.Empty<string>());

        claims.Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_StaticSource_EmitsConfiguredValue()
    {
        var json = """[{"claimType":"tenant","source":"static","value":"qb-engineer"}]""";
        var claims = ClaimMapper.ApplyMappings(json, BuildPrincipal(), Array.Empty<string>()).ToList();

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be("qb-engineer");
    }

    [Fact]
    public void ApplyMappings_UnknownSource_IsIgnored()
    {
        var json = """[{"claimType":"x","source":"bogus","value":"y"}]""";
        ClaimMapper.ApplyMappings(json, BuildPrincipal(), Array.Empty<string>()).Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_MissingClaimType_IsIgnored()
    {
        var json = """[{"source":"static","value":"y"}]""";
        ClaimMapper.ApplyMappings(json, BuildPrincipal(), Array.Empty<string>()).Should().BeEmpty();
    }

    [Fact]
    public void ApplyMappings_MultipleRulesInOrder_ReturnsEachEmission()
    {
        var json = """
        [
          {"claimType":"qb_role","source":"role","value":"Admin"},
          {"claimType":"tenant","source":"static","value":"qb-engineer"},
          {"claimType":"qb_company_id","source":"profile","value":"company_id"}
        ]
        """;
        var principal = BuildPrincipal(("company_id", "acme-42"));
        var claims = ClaimMapper.ApplyMappings(json, principal, new[] { "Admin" }).ToList();

        claims.Should().HaveCount(3);
        claims.Select(c => (c.Type, c.Value))
            .Should().BeEquivalentTo(new[]
            {
                ("qb_role", "Admin"),
                ("tenant", "qb-engineer"),
                ("qb_company_id", "acme-42"),
            });
    }
}
