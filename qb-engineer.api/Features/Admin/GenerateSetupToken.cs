using System.Security.Cryptography;

using MediatR;
using Microsoft.AspNetCore.Identity;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record GenerateSetupTokenCommand(int UserId) : IRequest<SetupTokenResponseModel>;

public record SetupTokenResponseModel(string Token, DateTime ExpiresAt);

public class GenerateSetupTokenHandler(
    UserManager<ApplicationUser> userManager) : IRequestHandler<GenerateSetupTokenCommand, SetupTokenResponseModel>
{
    public async Task<SetupTokenResponseModel> Handle(GenerateSetupTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new KeyNotFoundException($"User {request.UserId} not found");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        user.SetupToken = token;
        user.SetupTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        return new SetupTokenResponseModel(token, user.SetupTokenExpiresAt.Value);
    }
}
