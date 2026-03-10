using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Quotes;

public record GetQuotesQuery(int? CustomerId, QuoteStatus? Status) : IRequest<List<QuoteListItemModel>>;

public class GetQuotesHandler(IQuoteRepository repo)
    : IRequestHandler<GetQuotesQuery, List<QuoteListItemModel>>
{
    public async Task<List<QuoteListItemModel>> Handle(GetQuotesQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.CustomerId, request.Status, cancellationToken);
    }
}
