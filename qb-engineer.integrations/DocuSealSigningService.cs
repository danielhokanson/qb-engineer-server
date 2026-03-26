using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class DocuSealSigningService : IDocumentSigningService
{
    private readonly HttpClient _httpClient;
    private readonly DocuSealOptions _options;
    private readonly ILogger<DocuSealSigningService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public DocuSealSigningService(
        HttpClient httpClient,
        IOptions<DocuSealOptions> options,
        ILogger<DocuSealSigningService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _options.ApiKey);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/templates", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DocuSeal health check failed");
            return false;
        }
    }

    public async Task<int> CreateTemplateFromPdfAsync(string name, byte[] pdfBytes, CancellationToken ct)
    {
        _logger.LogInformation("DocuSeal CreateTemplate: {Name} ({Size} bytes)", name, pdfBytes.Length);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(pdfBytes), "files[]", $"{name}.pdf");
        content.Add(new StringContent(name), "name");

        var response = await _httpClient.PostAsync("/api/templates/pdf", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DocuSealTemplateResponse>(JsonOptions, ct);
        return result?.Id ?? throw new InvalidOperationException("DocuSeal returned no template ID");
    }

    public async Task<DocumentSigningSubmission> CreateSubmissionAsync(int templateId, string signerEmail, string signerName, CancellationToken ct)
    {
        _logger.LogInformation("DocuSeal CreateSubmission: template {TemplateId} for {Email}", templateId, signerEmail);

        var request = new DocuSealSubmissionRequest
        {
            TemplateId = templateId,
            SendEmail = false,
            Submitters =
            [
                new DocuSealSubmitter
                {
                    Email = signerEmail,
                    Name = signerName,
                    Role = "First Party",
                },
            ],
        };

        var response = await _httpClient.PostAsJsonAsync("/api/submissions", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<DocuSealSubmissionResponse>>(JsonOptions, ct);
        var submission = results?.FirstOrDefault()
            ?? throw new InvalidOperationException("DocuSeal returned no submission");

        return new DocumentSigningSubmission(submission.Id, submission.EmbedSrc);
    }

    public async Task<DocumentSigningMultiSubmission> CreateSubmissionFromPdfAsync(
        string templateName,
        byte[] pdfBytes,
        IReadOnlyList<SequentialSubmitter> submitters,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "DocuSeal CreateSubmissionFromPdf: '{Name}' ({Size} bytes), {Count} submitters",
            templateName, pdfBytes.Length, submitters.Count);

        // Step 1: Upload the filled PDF as a one-time template
        var templateId = await CreateTemplateFromPdfAsync(templateName, pdfBytes, ct);

        // Step 2: Create submission with all submitters in sequential order
        var submitterDtos = submitters
            .OrderBy(s => s.Order)
            .Select(s => new DocuSealSubmitter
            {
                Email = s.Email,
                Name = s.Name,
                Role = s.Role,
            })
            .ToList();

        var request = new DocuSealSubmissionRequest
        {
            TemplateId = templateId,
            SendEmail = false,
            Submitters = submitterDtos,
        };

        var response = await _httpClient.PostAsJsonAsync("/api/submissions", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<DocuSealSubmissionResponse>>(JsonOptions, ct)
            ?? throw new InvalidOperationException("DocuSeal returned no submission results");

        // DocuSeal returns submitters in order — zip with original submitters to build the result map
        var orderedSubmitters = submitters.OrderBy(s => s.Order).ToList();
        var byOrder = new Dictionary<int, SubmitterResult>();

        for (var i = 0; i < Math.Min(results.Count, orderedSubmitters.Count); i++)
        {
            var order = orderedSubmitters[i].Order;
            byOrder[order] = new SubmitterResult(results[i].Id, results[i].EmbedSrc);
        }

        return new DocumentSigningMultiSubmission(templateId, byOrder);
    }

    public async Task<byte[]> GetSignedPdfAsync(int submissionId, CancellationToken ct)
    {
        _logger.LogInformation("DocuSeal GetSignedPdf: submission {Id}", submissionId);

        var response = await _httpClient.GetAsync($"/api/submissions/{submissionId}", ct);
        response.EnsureSuccessStatusCode();

        var submission = await response.Content.ReadFromJsonAsync<DocuSealSubmissionDetailResponse>(JsonOptions, ct);
        var documentUrl = submission?.Documents?.FirstOrDefault()?.Url;

        if (string.IsNullOrEmpty(documentUrl))
            throw new InvalidOperationException($"No signed document found for submission {submissionId}");

        return await _httpClient.GetByteArrayAsync(documentUrl, ct);
    }

    public async Task<DocumentSigningSubmissionStatus> GetSubmissionStatusAsync(int submissionId, CancellationToken ct)
    {
        _logger.LogInformation("DocuSeal GetSubmissionStatus: submission {Id}", submissionId);

        var response = await _httpClient.GetAsync($"/api/submissions/{submissionId}", ct);
        response.EnsureSuccessStatusCode();

        var submission = await response.Content.ReadFromJsonAsync<DocuSealSubmissionDetailResponse>(JsonOptions, ct);
        return new DocumentSigningSubmissionStatus(
            submission?.Status ?? "unknown",
            submission?.CompletedAt);
    }

    public async Task DeleteTemplateAsync(int templateId, CancellationToken ct)
    {
        _logger.LogInformation("DocuSeal DeleteTemplate: {Id}", templateId);

        var response = await _httpClient.DeleteAsync($"/api/templates/{templateId}", ct);
        response.EnsureSuccessStatusCode();
    }

    // ─── DocuSeal API DTOs ───

    private sealed class DocuSealTemplateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class DocuSealSubmissionRequest
    {
        public int TemplateId { get; set; }
        public bool SendEmail { get; set; }
        public List<DocuSealSubmitter> Submitters { get; set; } = [];
    }

    private sealed class DocuSealSubmitter
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    private sealed class DocuSealSubmissionResponse
    {
        public int Id { get; set; }
        public string EmbedSrc { get; set; } = string.Empty;
    }

    private sealed class DocuSealSubmissionDetailResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public List<DocuSealDocument>? Documents { get; set; }
    }

    private sealed class DocuSealDocument
    {
        public string Url { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
    }
}
