using FluentValidation;
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

public class CreateAdminUserValidator : AbstractValidator<CreateAdminUserCommand>
{
    public CreateAdminUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Initials).MaximumLength(4).When(x => x.Initials is not null);
        RuleFor(x => x.AvatarColor).MaximumLength(20).When(x => x.AvatarColor is not null);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}

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
