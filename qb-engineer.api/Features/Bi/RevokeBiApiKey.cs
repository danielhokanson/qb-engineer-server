using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Bi;

public record RevokeBiApiKeyCommand(int Id) : IRequest;

public class RevokeBiApiKeyHandler(AppDbContext db)
    : IRequestHandler<RevokeBiApiKeyCommand>
{
    public async Task Handle(RevokeBiApiKeyCommand request, CancellationToken cancellationToken)
    {
        var key = await db.BiApiKeys
            .FirstOrDefaultAsync(k => k.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"BiApiKey {request.Id} not found");

        key.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }
}
