using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Quotes;

public record DeleteQuoteCommand(int Id) : IRequest;

public class DeleteQuoteHandler(IQuoteRepository repo)
    : IRequestHandler<DeleteQuoteCommand>
{
    public async Task Handle(DeleteQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quote {request.Id} not found");

        if (quote.Status != QuoteStatus.Draft)
            throw new InvalidOperationException("Only Draft quotes can be deleted");

        quote.DeletedAt = DateTimeOffset.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
