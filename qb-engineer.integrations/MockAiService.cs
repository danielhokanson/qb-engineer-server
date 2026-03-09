using Microsoft.Extensions.Logging;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class MockAiService : IAiService
{
    private readonly ILogger<MockAiService> _logger;

    public MockAiService(ILogger<MockAiService> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateTextAsync(string prompt, CancellationToken ct)
    {
        var truncated = prompt.Length > 80 ? prompt[..80] + "..." : prompt;
        _logger.LogInformation("[MockAI] GenerateText: {Prompt}", truncated);
        return Task.FromResult($"[Mock AI] Generated response for: {truncated}");
    }

    public Task<string> SummarizeAsync(string text, CancellationToken ct)
    {
        var summary = text.Length > 200 ? text[..200] + "..." : text;
        _logger.LogInformation("[MockAI] Summarize ({Length} chars)", text.Length);
        return Task.FromResult(summary);
    }

    public Task<List<AiSearchResult>> SmartSearchAsync(string naturalLanguageQuery, CancellationToken ct)
    {
        _logger.LogInformation("[MockAI] SmartSearch: {Query}", naturalLanguageQuery);
        return Task.FromResult(new List<AiSearchResult>());
    }

    public Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        _logger.LogInformation("[MockAI] IsAvailable — returning true");
        return Task.FromResult(true);
    }
}
