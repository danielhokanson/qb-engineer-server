using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using QBEngineer.Api.Features.Auth;
using QBEngineer.Data.Context;

namespace QBEngineer.Tests.Handlers.Auth;

public class ChangePasswordHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ChangePasswordHandler _handler;
    private readonly Faker _faker = new();

    public ChangePasswordHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new ChangePasswordHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCurrentPassword_ChangesPassword()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
        };

        _userManagerMock.Setup(x => x.FindByIdAsync("1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "OldPassword1!"))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.ChangePasswordAsync(user, "OldPassword1!", "NewPassword1!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand(1, "OldPassword1!", "NewPassword1!");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();

        _userManagerMock.Verify(x => x.ChangePasswordAsync(user, "OldPassword1!", "NewPassword1!"), Times.Once);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_InvalidCurrentPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new ApplicationUser
        {
            Id = 2,
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
        };

        _userManagerMock.Setup(x => x.FindByIdAsync("2"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "WrongPassword"))
            .ReturnsAsync(false);

        var command = new ChangePasswordCommand(2, "WrongPassword", "NewPassword1!");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Current password is incorrect");
    }
}
