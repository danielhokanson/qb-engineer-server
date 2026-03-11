using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IAiService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken ct);
    Task<string> SummarizeAsync(string text, CancellationToken ct);
    Task<List<AiSearchResult>> SmartSearchAsync(string naturalLanguageQuery, CancellationToken ct);
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct);
    Task<bool> IsAvailableAsync(CancellationToken ct);
}
