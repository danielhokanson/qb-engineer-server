using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record UpdateJobPositionCommand(int JobId, int Position) : IRequest<Unit>;

public class UpdateJobPositionHandler(AppDbContext db) : IRequestHandler<UpdateJobPositionCommand, Unit>
{
    public async Task<Unit> Handle(UpdateJobPositionCommand request, CancellationToken cancellationToken)
    {
        var job = await db.Jobs
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        job.BoardPosition = request.Position;

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
