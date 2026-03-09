using MediatR;
using Microsoft.AspNetCore.Identity;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record CreateAdminUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    string Password,
    string Role) : IRequest<AdminUserResponseModel>;

public class CreateAdminUserHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<CreateAdminUserCommand, AdminUserResponseModel>
{
    public async Task<AdminUserResponseModel> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Initials = request.Initials ?? GenerateInitials(request.FirstName, request.LastName),
            AvatarColor = request.AvatarColor ?? "#94a3b8",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await userManager.AddToRoleAsync(user, request.Role);

        return new AdminUserResponseModel(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            user.IsActive,
            [request.Role],
            user.CreatedAt);
    }

    private static string GenerateInitials(string firstName, string lastName)
    {
        var first = string.IsNullOrEmpty(firstName) ? "" : firstName[..1].ToUpper();
        var last = string.IsNullOrEmpty(lastName) ? "" : lastName[..1].ToUpper();
        return first + last;
    }
}
