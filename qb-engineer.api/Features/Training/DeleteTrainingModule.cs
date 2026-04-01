using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Training;

public record DeleteTrainingModuleCommand(int Id) : IRequest;

public class DeleteTrainingModuleHandler(AppDbContext db) : IRequestHandler<DeleteTrainingModuleCommand>
{
    public async Task Handle(DeleteTrainingModuleCommand request, CancellationToken ct)
    {
        var module = await db.TrainingModules.FirstOrDefaultAsync(m => m.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Training module {request.Id} not found.");

        module.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
