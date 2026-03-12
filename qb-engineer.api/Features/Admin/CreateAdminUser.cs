using System.Security.Cryptography;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record CreateAdminUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    string Role) : IRequest<CreateAdminUserResponseModel>;

public record CreateAdminUserResponseModel(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    bool IsActive,
    string[] Roles,
    DateTime CreatedAt,
    string SetupToken,
    DateTime SetupTokenExpiresAt);

public class CreateAdminUserValidator : AbstractValidator<CreateAdminUserCommand>
{
    public CreateAdminUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Initials).MaximumLength(4).When(x => x.Initials is not null);
        RuleFor(x => x.AvatarColor).MaximumLength(20).When(x => x.AvatarColor is not null);
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}

public class CreateAdminUserHandler(UserManager<ApplicationUser> userManager, IBarcodeService barcodeService)
    : IRequestHandler<CreateAdminUserCommand, CreateAdminUserResponseModel>
{
    public async Task<CreateAdminUserResponseModel> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
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

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await userManager.AddToRoleAsync(user, request.Role);

        await barcodeService.CreateBarcodeAsync(
            BarcodeEntityType.User, user.Id, $"{user.Id:D6}", cancellationToken);

        // Generate short setup code (XXXX-XXXX) — easy to read aloud or write down
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I to avoid confusion
        var bytes = RandomNumberGenerator.GetBytes(8);
        var code = new char[8];
        for (var i = 0; i < 8; i++)
            code[i] = chars[bytes[i] % chars.Length];
        var token = $"{new string(code, 0, 4)}-{new string(code, 4, 4)}";

        user.SetupToken = token;
        user.SetupTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        return new CreateAdminUserResponseModel(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            user.IsActive,
            [request.Role],
            user.CreatedAt,
            token,
            user.SetupTokenExpiresAt.Value);
    }

    private static string GenerateInitials(string firstName, string lastName)
    {
        var first = string.IsNullOrEmpty(firstName) ? "" : firstName[..1].ToUpper();
        var last = string.IsNullOrEmpty(lastName) ? "" : lastName[..1].ToUpper();
        return first + last;
    }
}
