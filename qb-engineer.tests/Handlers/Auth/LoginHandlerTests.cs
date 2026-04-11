using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using QBEngineer.Api.Features.Auth;
using QBEngineer.Data.Context;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Handlers.Auth;

public class LoginHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly LoginHandler _handler;
    private readonly Faker _faker = new();

    public LoginHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-characters-long!!",
                ["Jwt:Issuer"] = "qb-engineer-test",
                ["Jwt:Audience"] = "qb-engineer-ui-test",
            })
            .Build();

        _db = TestDbContextFactory.Create();
        _handler = new LoginHandler(_userManagerMock.Object, _config, _db);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenAndUser()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            Initials = "JD",
            AvatarColor = "#FF0000",
            IsActive = true,
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "ValidPassword1!"))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        var command = new LoginCommand(user.Email, "ValidPassword1!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be(user.Email);
        result.User.FirstName.Should().Be(user.FirstName);
        result.User.LastName.Should().Be(user.LastName);
        result.User.Roles.Should().Contain("Admin");
        result.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsUnauthorized()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new LoginCommand("nonexistent@example.com", "password");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorized()
    {
        var user = new ApplicationUser
        {
            Id = 2,
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            IsActive = true,
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(false);

        var command = new LoginCommand(user.Email, "WrongPassword");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_LockedOutUser_ThrowsUnauthorized()
    {
        var user = new ApplicationUser
        {
            Id = 3,
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            IsActive = false,
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var command = new LoginCommand(user.Email, "AnyPassword");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }
}
