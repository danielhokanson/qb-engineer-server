using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Estimates;

public record ConvertEstimateToQuoteCommand(int EstimateId) : IRequest<QuoteListItemModel>;

public class ConvertEstimateToQuoteHandler(AppDbContext db)
    : IRequestHandler<ConvertEstimateToQuoteCommand, QuoteListItemModel>
{
    public async Task<QuoteListItemModel> Handle(ConvertEstimateToQuoteCommand request, CancellationToken ct)
    {
        var estimate = await db.Estimates
            .Include(e => e.Customer)
            .FirstOrDefaultAsync(e => e.Id == request.EstimateId && e.DeletedAt == null, ct)
            ?? throw new KeyNotFoundException($"Estimate {request.EstimateId} not found.");

        if (estimate.ConvertedToQuoteId.HasValue)
            throw new InvalidOperationException("Estimate has already been converted to a quote.");

        var quoteNumber = await GenerateQuoteNumberAsync(ct);
        var quote = new Quote
        {
            QuoteNumber = quoteNumber,
            CustomerId = estimate.CustomerId,
            Status = QuoteStatus.Draft,
            Notes = estimate.Description ?? estimate.Notes,
            ExpirationDate = estimate.ValidUntil,
            TaxRate = 0,
        };

        db.Quotes.Add(quote);
        await db.SaveChangesAsync(ct);

        estimate.ConvertedToQuoteId = quote.Id;
        estimate.ConvertedAt = DateTimeOffset.UtcNow;
        estimate.Status = EstimateStatus.Accepted;
        await db.SaveChangesAsync(ct);

        return new QuoteListItemModel(
            quote.Id,
            quote.QuoteNumber,
            estimate.CustomerId,
            estimate.Customer.Name,
            quote.Status.ToString(),
            0,
            0m,
            quote.ExpirationDate,
            quote.CreatedAt);
    }

    private async Task<string> GenerateQuoteNumberAsync(CancellationToken ct)
    {
        var count = await db.Quotes.CountAsync(ct);
        return $"Q-{(count + 1):D5}";
    }
}
