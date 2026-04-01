using MediatR;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Parts;

public sealed record DeleteProcessStepCommand(int PartId, int StepId) : IRequest;

public sealed class DeleteProcessStepHandler(IPartRepository repo) : IRequestHandler<DeleteProcessStepCommand>
{
    public async Task Handle(DeleteProcessStepCommand request, CancellationToken cancellationToken)
    {
        var step = await repo.FindProcessStepAsync(request.StepId, cancellationToken)
            ?? throw new KeyNotFoundException($"Process step {request.StepId} not found");

        if (step.PartId != request.PartId)
            throw new KeyNotFoundException($"Process step {request.StepId} does not belong to part {request.PartId}");

        step.DeletedAt = DateTimeOffset.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
