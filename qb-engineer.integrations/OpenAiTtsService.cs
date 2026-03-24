using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class OpenAiTtsService(
    IHttpClientFactory httpClientFactory,
    IOptions<TtsOptions> options,
    ILogger<OpenAiTtsService> logger) : ITtsService
{
    private readonly TtsOptions _opts = options.Value;

    public async Task<byte[]> GenerateSpeechAsync(string text, CancellationToken ct = default)
    {
        logger.LogInformation("OpenAiTts: generating speech ({Length} chars)", text.Length);

        using var client = httpClientFactory.CreateClient("openai-tts");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _opts.ApiKey);

        var payload = new
        {
            model = _opts.Model,
            input = text,
            voice = _opts.Voice,
        };

        using var response = await client.PostAsJsonAsync(
            $"{_opts.BaseUrl.TrimEnd('/')}/audio/speech",
            payload,
            ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
