using System.Text.Json;

using FluentValidation;
using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

public record UpdateScopeCommand(
    int Id,
    string DisplayName,
    string Description,
    string ClaimMappingsJson,
    string? ResourcesCsv,
    bool IsActive,
    int ActorUserId,
    string? ActorIp) : IRequest<Unit>;

public class UpdateScopeValidator : AbstractValidator<UpdateScopeCommand>
{
    public UpdateScopeValidator()
    {
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

public class UpdateScopeHandler(AppDbContext db, IOidcAuditService audit)
    : IRequestHandler<UpdateScopeCommand, Unit>
{
    public async Task<Unit> Handle(UpdateScopeCommand request, CancellationToken ct)
    {
        var scope = await db.OidcCustomScopes.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"OIDC scope {request.Id} not found.");

        if (scope.IsSystem)
        {
            throw new InvalidOperationException("System scopes cannot be edited.");
        }

        scope.DisplayName = request.DisplayName;
        scope.Description = request.Description;
        scope.ClaimMappingsJson = request.ClaimMappingsJson;
        scope.ResourcesCsv = request.ResourcesCsv;
        scope.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ScopeUpdated,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            scopeName: scope.Name,
            ct: ct);

        return Unit.Value;
    }
}
