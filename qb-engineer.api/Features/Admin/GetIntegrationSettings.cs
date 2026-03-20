using MediatR;

using Microsoft.Extensions.Options;

using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Admin;

public record GetIntegrationSettingsQuery : IRequest<List<IntegrationStatusModel>>;

public class GetIntegrationSettingsHandler(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<MinioOptions> minioOptions,
    IOptions<UspsOptions> uspsOptions,
    IOptions<DocuSealOptions> docuSealOptions,
    IOptions<OllamaOptions> ollamaOptions) : IRequestHandler<GetIntegrationSettingsQuery, List<IntegrationStatusModel>>
{
    public Task<List<IntegrationStatusModel>> Handle(GetIntegrationSettingsQuery request, CancellationToken ct)
    {
        var smtp = smtpOptions.Value;
        var minio = minioOptions.Value;
        var usps = uspsOptions.Value;
        var docuSeal = docuSealOptions.Value;
        var ollama = ollamaOptions.Value;

        var integrations = new List<IntegrationStatusModel>
        {
            new(
                Provider: "smtp",
                Name: "SMTP Email",
                Description: "Outbound email notifications and invoices",
                Icon: "email",
                IsConfigured: !string.IsNullOrEmpty(smtp.Host) && smtp.Host != "localhost",
                Fields:
                [
                    new("Host", "SMTP Host", smtp.Host, false, true),
                    new("Port", "Port", smtp.Port.ToString(), false, true, "number"),
                    new("Username", "Username", smtp.Username ?? "", false, false),
                    new("Password", "Password", MaskSecret(smtp.Password), true, false, "password"),
                    new("UseSsl", "Use SSL/TLS", smtp.UseSsl.ToString(), false, false, "toggle"),
                    new("FromAddress", "From Address", smtp.FromAddress, false, true, "email"),
                    new("FromName", "From Name", smtp.FromName, false, false),
                ]),
            new(
                Provider: "minio",
                Name: "MinIO Storage",
                Description: "S3-compatible file storage for documents and attachments",
                Icon: "cloud_upload",
                IsConfigured: !string.IsNullOrEmpty(minio.Endpoint),
                Fields:
                [
                    new("Endpoint", "Endpoint", minio.Endpoint, false, true),
                    new("AccessKey", "Access Key", minio.AccessKey, false, true),
                    new("SecretKey", "Secret Key", MaskSecret(minio.SecretKey), true, true, "password"),
                    new("UseSsl", "Use SSL", minio.UseSsl.ToString(), false, false, "toggle"),
                ]),
            new(
                Provider: "usps",
                Name: "USPS Address Validation",
                Description: "USPS Addresses API v3 for address verification (free with Business account)",
                Icon: "local_post_office",
                IsConfigured: !string.IsNullOrEmpty(usps.ConsumerKey),
                Fields:
                [
                    new("ConsumerKey", "Consumer Key", usps.ConsumerKey, false, true),
                    new("ConsumerSecret", "Consumer Secret", MaskSecret(usps.ConsumerSecret), true, true, "password"),
                ]),
            new(
                Provider: "docuseal",
                Name: "DocuSeal Document Signing",
                Description: "Electronic document signing for employee forms and contracts",
                Icon: "draw",
                IsConfigured: !string.IsNullOrEmpty(docuSeal.ApiKey),
                Fields:
                [
                    new("BaseUrl", "Base URL", docuSeal.BaseUrl, false, true),
                    new("ApiKey", "API Key", MaskSecret(docuSeal.ApiKey), true, true, "password"),
                    new("WebhookSecret", "Webhook Secret", MaskSecret(docuSeal.WebhookSecret), true, false, "password"),
                ]),
            new(
                Provider: "ollama",
                Name: "AI Assistant (Ollama)",
                Description: "Self-hosted AI for smart search, drafting, and document Q&A",
                Icon: "psychology",
                IsConfigured: !string.IsNullOrEmpty(ollama.BaseUrl),
                Fields:
                [
                    new("BaseUrl", "Base URL", ollama.BaseUrl, false, true),
                    new("Model", "Model Name", ollama.Model, false, true),
                    new("TimeoutSeconds", "Timeout (seconds)", ollama.TimeoutSeconds.ToString(), false, false, "number"),
                ]),
        };

        return Task.FromResult(integrations);
    }

    private static string MaskSecret(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Length <= 4) return new string('*', value.Length);
        return new string('*', value.Length - 4) + value[^4..];
    }
}
