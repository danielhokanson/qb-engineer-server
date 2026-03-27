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
    IOptions<OllamaOptions> ollamaOptions,
    IOptions<UpsOptions> upsOptions,
    IOptions<FedExOptions> fedExOptions,
    IOptions<DhlOptions> dhlOptions,
    IOptions<XeroOptions> xeroOptions,
    IOptions<FreshBooksOptions> freshBooksOptions,
    IOptions<SageOptions> sageOptions,
    IOptions<NetSuiteOptions> netSuiteOptions,
    IOptions<WaveOptions> waveOptions,
    IOptions<ZohoOptions> zohoOptions) : IRequestHandler<GetIntegrationSettingsQuery, List<IntegrationStatusModel>>
{
    public Task<List<IntegrationStatusModel>> Handle(GetIntegrationSettingsQuery request, CancellationToken ct)
    {
        var smtp = smtpOptions.Value;
        var minio = minioOptions.Value;
        var usps = uspsOptions.Value;
        var docuSeal = docuSealOptions.Value;
        var ollama = ollamaOptions.Value;
        var ups = upsOptions.Value;
        var fedEx = fedExOptions.Value;
        var dhl = dhlOptions.Value;
        var xero = xeroOptions.Value;
        var freshBooks = freshBooksOptions.Value;
        var sage = sageOptions.Value;
        var netSuite = netSuiteOptions.Value;
        var wave = waveOptions.Value;
        var zoho = zohoOptions.Value;

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
                ],
                Category: "service"),
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
                ],
                Category: "service"),
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
                ],
                Category: "service"),
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
                ],
                Category: "service"),
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
                ],
                Category: "service"),
            new(
                Provider: "ups",
                Name: "UPS",
                Description: "UPS shipping rate shopping, label creation, and tracking",
                Icon: "local_shipping",
                IsConfigured: !string.IsNullOrEmpty(ups.ClientId),
                Fields:
                [
                    new("ClientId", "Client ID", ups.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(ups.ClientSecret), true, true, "password"),
                    new("AccountNumber", "Account Number", ups.AccountNumber, false, false),
                    new("Environment", "Environment", ups.Environment, false, false),
                ],
                Category: "shipping"),
            new(
                Provider: "fedex",
                Name: "FedEx",
                Description: "FedEx shipping rate shopping, label creation, and tracking",
                Icon: "local_shipping",
                IsConfigured: !string.IsNullOrEmpty(fedEx.ClientId),
                Fields:
                [
                    new("ClientId", "Client ID", fedEx.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(fedEx.ClientSecret), true, true, "password"),
                    new("AccountNumber", "Account Number", fedEx.AccountNumber, false, false),
                    new("Environment", "Environment", fedEx.Environment, false, false),
                ],
                Category: "shipping"),
            new(
                Provider: "dhl",
                Name: "DHL Express",
                Description: "DHL Express international shipping",
                Icon: "flight_takeoff",
                IsConfigured: !string.IsNullOrEmpty(dhl.ApiKey),
                Fields:
                [
                    new("ApiKey", "API Key", dhl.ApiKey, false, true),
                    new("ApiSecret", "API Secret", MaskSecret(dhl.ApiSecret), true, true, "password"),
                    new("AccountNumber", "Account Number", dhl.AccountNumber, false, false),
                    new("BaseUrl", "Base URL", dhl.BaseUrl, false, false),
                ],
                Category: "shipping"),
            new(
                Provider: "xero",
                Name: "Xero",
                Description: "Cloud accounting with multi-currency support",
                Icon: "account_balance_wallet",
                IsConfigured: !string.IsNullOrEmpty(xero.ClientId),
                Fields:
                [
                    new("ClientId", "Client ID", xero.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(xero.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", xero.RedirectUri, false, false),
                ],
                Category: "accounting"),
            new(
                Provider: "freshbooks",
                Name: "FreshBooks",
                Description: "Small business invoicing and accounting",
                Icon: "receipt_long",
                IsConfigured: !string.IsNullOrEmpty(freshBooks.ClientId),
                Fields:
                [
                    new("ClientId", "Client ID", freshBooks.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(freshBooks.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", freshBooks.RedirectUri, false, false),
                ],
                Category: "accounting"),
            new(
                Provider: "sage",
                Name: "Sage Business Cloud",
                Description: "Sage Business Cloud Accounting",
                Icon: "business",
                IsConfigured: !string.IsNullOrEmpty(sage.ClientId),
                Fields:
                [
                    new("ClientId", "Client ID", sage.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(sage.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", sage.RedirectUri, false, false),
                    new("CountryCode", "Country Code", sage.CountryCode, false, false),
                ],
                Category: "accounting"),
            new(
                Provider: "netsuite",
                Name: "NetSuite",
                Description: "NetSuite ERP (Token-Based Authentication)",
                Icon: "corporate_fare",
                IsConfigured: !string.IsNullOrEmpty(netSuite.AccountId),
                Fields:
                [
                    new("AccountId", "Account ID", netSuite.AccountId, false, true),
                    new("ConsumerKey", "Consumer Key", netSuite.ConsumerKey, false, true),
                    new("ConsumerSecret", "Consumer Secret", MaskSecret(netSuite.ConsumerSecret), true, true, "password"),
                    new("TokenId", "Token ID", netSuite.TokenId, false, true),
                    new("TokenSecret", "Token Secret", MaskSecret(netSuite.TokenSecret), true, true, "password"),
                ],
                Category: "accounting"),
            new(
                Provider: "wave",
                Name: "Wave",
                Description: "Free small business accounting",
                Icon: "waves",
                IsConfigured: !string.IsNullOrEmpty(wave.AccessToken),
                Fields:
                [
                    new("AccessToken", "Access Token", MaskSecret(wave.AccessToken), true, true, "password"),
                    new("BusinessId", "Business ID", wave.BusinessId, false, true),
                ],
                Category: "accounting"),
            new(
                Provider: "zoho",
                Name: "Zoho Books",
                Description: "Zoho Books accounting and invoicing",
                Icon: "menu_book",
                IsConfigured: !string.IsNullOrEmpty(zoho.ClientId),
                Fields:
                [
                    new("ClientId", "Client ID", zoho.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(zoho.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", zoho.RedirectUri, false, false),
                    new("OrganizationId", "Organization ID", zoho.OrganizationId, false, true),
                    new("DataCenter", "Data Center", zoho.DataCenter, false, false),
                ],
                Category: "accounting"),
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
