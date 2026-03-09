using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ISearchRepository
{
    Task<List<SearchResultModel>> SearchAsync(string term, int limit, CancellationToken ct);
}
