using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.PriceLists;

public record DeletePriceListEntryCommand(int Id) : IRequest;

public class DeletePriceListEntryHandler(IPriceListRepository repo)
    : IRequestHandler<DeletePriceListEntryCommand>
{
    public async Task Handle(DeletePriceListEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repo.FindEntryAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Price list entry {request.Id} not found");

        entry.DeletedAt = DateTimeOffset.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
