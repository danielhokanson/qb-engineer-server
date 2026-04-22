using FluentAssertions;

using QBEngineer.Api.Features.Oidc;

namespace QBEngineer.Tests.Handlers.Oidc;

public class OidcCryptoTests
{
    [Fact]
    public void GenerateTicket_Prefix_IsOidt()
    {
        var ticket = OidcCrypto.GenerateTicket();
        ticket.Should().StartWith("oidt_");
        ticket.Length.Should().BeGreaterThanOrEqualTo(40);
    }

    [Fact]
    public void GenerateClientSecret_Prefix_IsOids()
    {
        OidcCrypto.GenerateClientSecret().Should().StartWith("oids_");
    }

    [Fact]
    public void GenerateRegistrationAccessToken_Prefix_IsOidr()
    {
        OidcCrypto.GenerateRegistrationAccessToken().Should().StartWith("oidr_");
    }

    [Fact]
    public void GenerateTicket_TwoCalls_AreUnique()
    {
        OidcCrypto.GenerateTicket().Should().NotBe(OidcCrypto.GenerateTicket());
    }

    [Fact]
    public void HashSha256_SameInput_ProducesSameHash()
    {
        var input = "oidt_abc123";
        OidcCrypto.HashSha256(input).Should().Be(OidcCrypto.HashSha256(input));
    }

    [Fact]
    public void HashSha256_DifferentInputs_ProduceDifferentHashes()
    {
        OidcCrypto.HashSha256("a").Should().NotBe(OidcCrypto.HashSha256("b"));
    }

    [Fact]
    public void HashSha256_IsHexLowercase_64Chars()
    {
        var hash = OidcCrypto.HashSha256("anything");
        hash.Should().MatchRegex("^[a-f0-9]{64}$");
    }

    [Fact]
    public void ConstantTimeEquals_Equal_ReturnsTrue()
    {
        OidcCrypto.ConstantTimeEquals("abcdef", "abcdef").Should().BeTrue();
    }

    [Fact]
    public void ConstantTimeEquals_DifferentLength_ReturnsFalse()
    {
        OidcCrypto.ConstantTimeEquals("abc", "abcd").Should().BeFalse();
    }

    [Fact]
    public void ConstantTimeEquals_DifferentContent_ReturnsFalse()
    {
        OidcCrypto.ConstantTimeEquals("abcdef", "abcdeg").Should().BeFalse();
    }
}
