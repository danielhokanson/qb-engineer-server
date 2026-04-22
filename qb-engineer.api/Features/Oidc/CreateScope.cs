using System.Text.Json;

using FluentValidation;
using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

public record CreateScopeCommand(
    string Name,
    string DisplayName,
    string Description,
    string ClaimMappingsJson,
    string? ResourcesCsv,
    int ActorUserId,
    string? ActorIp) : IRequest<int>;

public class CreateScopeValidator : AbstractValidator<CreateScopeCommand>
{
    public CreateScopeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_:.\-]+$")
            .WithMessage("Scope name must contain only letters, digits, underscore, colon, dot, or hyphen.");
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ClaimMappingsJson).Must(BeValidJsonArray)
            .WithMessage("ClaimMappingsJson must parse as a JSON array.");
    }

    private static bool BeValidJsonArray(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public class CreateScopeHandler(AppDbContext db, IOidcAuditService audit)
    : IRequestHandler<CreateScopeCommand, int>
{
    public async Task<int> Handle(CreateScopeCommand request, CancellationToken ct)
    {
        var exists = await db.OidcCustomScopes.AnyAsync(s => s.Name == request.Name, ct);
        if (exists)
        {
            throw new InvalidOperationException($"Scope '{request.Name}' already exists.");
        }

        var scope = new OidcCustomScope
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            ClaimMappingsJson = request.ClaimMappingsJson,
            ResourcesCsv = request.ResourcesCsv,
            IsSystem = false,
            IsActive = true,
        };
        db.OidcCustomScopes.Add(scope);
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ScopeCreated,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            scopeName: request.Name,
            ct: ct);

        return scope.Id;
    }
}
