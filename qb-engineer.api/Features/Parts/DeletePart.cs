using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Parts;

public sealed record DeletePartCommand(int Id) : IRequest;

public sealed class DeletePartHandler(IPartRepository repo)
    : IRequestHandler<DeletePartCommand>
{
    public async Task Handle(DeletePartCommand request, CancellationToken cancellationToken)
    {
        var part = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.Id} not found");

        part.DeletedAt = DateTime.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
