using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.PriceLists;

public record DeletePriceListCommand(int Id) : IRequest;

public class DeletePriceListHandler(IPriceListRepository repo)
    : IRequestHandler<DeletePriceListCommand>
{
    public async Task Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Price list {request.Id} not found");

        priceList.DeletedAt = DateTime.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
