using MediatR;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Estimates;

public record DeleteEstimateCommand(int Id) : IRequest;

public class DeleteEstimateHandler(AppDbContext db) : IRequestHandler<DeleteEstimateCommand>
{
    public async Task Handle(DeleteEstimateCommand request, CancellationToken ct)
    {
        var estimate = await db.Estimates.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"Estimate {request.Id} not found.");

        if (estimate.DeletedAt != null)
            throw new KeyNotFoundException($"Estimate {request.Id} not found.");

        estimate.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
