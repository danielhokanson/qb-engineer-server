using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Search;

public record GlobalSearchQuery(string Term, int Limit = 20) : IRequest<List<SearchResultModel>>;

public class GlobalSearchHandler(ISearchRepository repo) : IRequestHandler<GlobalSearchQuery, List<SearchResultModel>>
{
    public Task<List<SearchResultModel>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        return repo.SearchAsync(request.Term, request.Limit, cancellationToken);
    }
}
