using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Quotes;

public record AcceptQuoteCommand(int Id) : IRequest;

public class AcceptQuoteHandler(IQuoteRepository repo)
    : IRequestHandler<AcceptQuoteCommand>
{
    public async Task Handle(AcceptQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Quote {request.Id} not found");

        if (quote.Status != QuoteStatus.Sent)
            throw new InvalidOperationException("Only Sent quotes can be accepted");

        quote.Status = QuoteStatus.Accepted;
        quote.AcceptedDate = DateTimeOffset.UtcNow;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
