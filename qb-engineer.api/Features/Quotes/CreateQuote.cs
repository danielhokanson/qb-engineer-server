using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Quotes;

public record CreateQuoteCommand(
    int CustomerId,
    int? ShippingAddressId,
    DateTime? ExpirationDate,
    string? Notes,
    decimal TaxRate,
    List<CreateQuoteLineModel> Lines) : IRequest<QuoteListItemModel>;

public class CreateQuoteValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).LessThan(1);
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateQuoteHandler(IQuoteRepository repo, ICustomerRepository customerRepo)
    : IRequestHandler<CreateQuoteCommand, QuoteListItemModel>
{
    public async Task<QuoteListItemModel> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepo.FindAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        var quoteNumber = await repo.GenerateNextQuoteNumberAsync(cancellationToken);

        var quote = new Quote
        {
            QuoteNumber = quoteNumber,
            CustomerId = request.CustomerId,
            ShippingAddressId = request.ShippingAddressId,
            ExpirationDate = request.ExpirationDate,
            Notes = request.Notes,
            TaxRate = request.TaxRate,
        };

        var lineNumber = 1;
        foreach (var line in request.Lines)
        {
            quote.Lines.Add(new QuoteLine
            {
                PartId = line.PartId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineNumber = lineNumber++,
                Notes = line.Notes,
            });
        }

        await repo.AddAsync(quote, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        var total = quote.Lines.Sum(l => l.Quantity * l.UnitPrice);

        return new QuoteListItemModel(
            quote.Id, quote.QuoteNumber, quote.CustomerId, customer.Name,
            quote.Status.ToString(), quote.Lines.Count, total,
            quote.ExpirationDate, quote.CreatedAt);
    }
}
