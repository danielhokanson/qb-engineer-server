using System.Security.Cryptography;
using System.Text.Json;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Bi;

public record CreateBiApiKeyCommand(CreateBiApiKeyRequestModel Model) : IRequest<CreateBiApiKeyResponseModel>;

public class CreateBiApiKeyValidator : AbstractValidator<CreateBiApiKeyCommand>
{
    public CreateBiApiKeyValidator()
    {
        RuleFor(x => x.Model.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateBiApiKeyHandler(AppDbContext db)
    : IRequestHandler<CreateBiApiKeyCommand, CreateBiApiKeyResponseModel>
{
    public async Task<CreateBiApiKeyResponseModel> Handle(
        CreateBiApiKeyCommand request, CancellationToken cancellationToken)
    {
        var model = request.Model;

        // Generate a random API key
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var plaintextKey = $"qbe_{Convert.ToBase64String(keyBytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
        var keyPrefix = plaintextKey[..12];

        // Hash the key for storage
        var hasher = new PasswordHasher<object>();
        var keyHash = hasher.HashPassword(null!, plaintextKey);

        var apiKey = new BiApiKey
        {
            Name = model.Name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            ExpiresAt = model.ExpiresAt,
            AllowedEntitySetsJson = model.AllowedEntitySets != null
                ? JsonSerializer.Serialize(model.AllowedEntitySets) : null,
            AllowedIpsJson = model.AllowedIps != null
                ? JsonSerializer.Serialize(model.AllowedIps) : null,
        };

        db.BiApiKeys.Add(apiKey);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateBiApiKeyResponseModel
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            KeyPrefix = apiKey.KeyPrefix,
            PlaintextKey = plaintextKey,
            ExpiresAt = apiKey.ExpiresAt,
        };
    }
}
