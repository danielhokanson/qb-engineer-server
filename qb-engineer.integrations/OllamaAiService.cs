using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaAiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public OllamaAiService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaAiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken ct)
    {
        _logger.LogInformation("Ollama GenerateText ({Model}): {Prompt}",
            _options.Model, prompt.Length > 80 ? prompt[..80] + "..." : prompt);

        var request = new OllamaGenerateRequest
        {
            Model = _options.Model,
            Prompt = prompt,
            Stream = false,
        };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(JsonOptions, ct);
        return result?.Response ?? string.Empty;
    }

    public async Task<string> SummarizeAsync(string text, CancellationToken ct)
    {
        var prompt = $"""
            Summarize the following text concisely in 2-3 sentences. Focus on the key facts and actionable information.

            Text:
            {text}

            Summary:
            """;

        return await GenerateTextAsync(prompt, ct);
    }

    public async Task<List<AiSearchResult>> SmartSearchAsync(string naturalLanguageQuery, CancellationToken ct)
    {
        _logger.LogInformation("Ollama SmartSearch: {Query}", naturalLanguageQuery);

        // Smart search uses the LLM to interpret the query and generate structured search terms.
        // Without pgvector/RAG, we extract keywords the caller can use for full-text search.
        var prompt = $"""
            You are a search assistant for a manufacturing operations platform. Given a natural language query, extract the most relevant search keywords.
            Return ONLY a JSON array of keyword strings, nothing else. Example: ["keyword1", "keyword2"]

            Query: {naturalLanguageQuery}
            """;

        var response = await GenerateTextAsync(prompt, ct);

        // For now, return empty results — full RAG/pgvector search is a future enhancement.
        // The keywords could be used by the caller to feed into existing full-text search.
        _logger.LogInformation("Ollama SmartSearch keywords: {Response}", response);
        return [];
    }

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct)
    {
        _logger.LogInformation("Ollama GetEmbedding ({Length} chars)", text.Length);

        var request = new OllamaEmbeddingRequest
        {
            Model = _options.EmbeddingModel,
            Prompt = text,
        };

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(JsonOptions, ct);
        return result?.Embedding ?? [];
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama health check failed");
            return false;
        }
    }

    // ─── Ollama API DTOs ───

    private sealed class OllamaGenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; set; }
        public string? System { get; set; }
    }

    private sealed class OllamaGenerateResponse
    {
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
        public int EvalCount { get; set; }
    }

    private sealed class OllamaEmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }

    private sealed class OllamaEmbeddingResponse
    {
        public float[] Embedding { get; set; } = [];
    }
}
