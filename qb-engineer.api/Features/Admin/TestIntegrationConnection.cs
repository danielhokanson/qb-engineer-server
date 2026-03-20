using MediatR;

using Microsoft.Extensions.Logging;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Admin;

public record TestIntegrationConnectionCommand(string Provider) : IRequest<TestIntegrationResultModel>;

public class TestIntegrationConnectionHandler(
    IEmailService emailService,
    IStorageService storageService,
    IAddressValidationService addressValidationService,
    IDocumentSigningService documentSigningService,
    IAiService aiService,
    ILogger<TestIntegrationConnectionHandler> logger) : IRequestHandler<TestIntegrationConnectionCommand, TestIntegrationResultModel>
{
    public async Task<TestIntegrationResultModel> Handle(TestIntegrationConnectionCommand request, CancellationToken ct)
    {
        try
        {
            var (success, message) = request.Provider switch
            {
                "smtp" => await TestSmtp(ct),
                "minio" => await TestMinio(ct),
                "usps" => await TestUsps(ct),
                "docuseal" => await TestDocuSeal(ct),
                "ollama" => await TestOllama(ct),
                _ => throw new KeyNotFoundException($"Unknown integration provider: {request.Provider}")
            };

            logger.LogInformation("[Integration Test] {Provider}: {Success} — {Message}", request.Provider, success, message);
            return new TestIntegrationResultModel(success, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Integration Test] {Provider} failed with exception", request.Provider);
            return new TestIntegrationResultModel(false, $"Connection test failed: {ex.Message}");
        }
    }

    private async Task<(bool, string)> TestSmtp(CancellationToken ct)
    {
        var success = await emailService.TestConnectionAsync(ct);
        return (success, success ? "SMTP connection successful" : "SMTP connection failed — check host, port, and credentials");
    }

    private async Task<(bool, string)> TestMinio(CancellationToken ct)
    {
        var success = await storageService.TestConnectionAsync(ct);
        return (success, success ? "MinIO connection successful" : "MinIO connection failed — check endpoint and credentials");
    }

    private async Task<(bool, string)> TestUsps(CancellationToken ct)
    {
        var success = await addressValidationService.TestConnectionAsync(ct);
        return (success, success ? "USPS API connection successful" : "USPS connection failed — check Consumer Key and Consumer Secret");
    }

    private async Task<(bool, string)> TestDocuSeal(CancellationToken ct)
    {
        var success = await documentSigningService.IsAvailableAsync(ct);
        return (success, success ? "DocuSeal connection successful" : "DocuSeal connection failed — check Base URL and API Key");
    }

    private async Task<(bool, string)> TestOllama(CancellationToken ct)
    {
        var success = await aiService.IsAvailableAsync(ct);
        return (success, success ? "Ollama AI connection successful" : "Ollama connection failed — check Base URL and ensure the AI container is running");
    }
}
