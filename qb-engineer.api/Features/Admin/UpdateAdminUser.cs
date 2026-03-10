using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record UpdateAdminUserCommand(
    int Id,
    string? FirstName,
    string? LastName,
    string? Initials,
    string? AvatarColor,
    bool? IsActive,
    string? Role) : IRequest<AdminUserResponseModel>;

public class UpdateAdminUserValidator : AbstractValidator<UpdateAdminUserCommand>
{
    public UpdateAdminUserValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => x.FirstName is not null);
        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName is not null);
        RuleFor(x => x.Initials).MaximumLength(4).When(x => x.Initials is not null);
        RuleFor(x => x.AvatarColor).MaximumLength(20).When(x => x.AvatarColor is not null);
        RuleFor(x => x.Role).MaximumLength(50).When(x => x.Role is not null);
    }
}

public class UpdateAdminUserHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<UpdateAdminUserCommand, AdminUserResponseModel>
{
    public async Task<AdminUserResponseModel> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User with ID {request.Id} not found.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Initials is not null) user.Initials = request.Initials;
        if (request.AvatarColor is not null) user.AvatarColor = request.AvatarColor;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        if (request.Role is not null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, request.Role);
        }

        var roles = await userManager.GetRolesAsync(user);

        return new AdminUserResponseModel(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            user.IsActive,
            roles.ToArray(),
            user.CreatedAt);
    }
}
