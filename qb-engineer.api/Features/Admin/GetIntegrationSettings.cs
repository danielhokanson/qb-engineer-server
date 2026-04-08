using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Admin;

public record GetIntegrationSettingsQuery : IRequest<IntegrationSettingsResult>;

public class GetIntegrationSettingsHandler(
    IConfiguration configuration,
    IOptions<SmtpOptions> smtpOptions,
    IOptions<MinioOptions> minioOptions,
    IOptions<UspsOptions> uspsOptions,
    IOptions<DocuSealOptions> docuSealOptions,
    IOptions<AiOptions> ollamaOptions,
    IOptions<UpsOptions> upsOptions,
    IOptions<FedExOptions> fedExOptions,
    IOptions<DhlOptions> dhlOptions,
    IOptions<StampsOptions> stampsOptions,
    IOptions<XeroOptions> xeroOptions,
    IOptions<FreshBooksOptions> freshBooksOptions,
    IOptions<SageOptions> sageOptions,
    IOptions<NetSuiteOptions> netSuiteOptions,
    IOptions<WaveOptions> waveOptions,
    IOptions<ZohoOptions> zohoOptions,
    IOptions<LocalStorageOptions> localStorageOptions) : IRequestHandler<GetIntegrationSettingsQuery, IntegrationSettingsResult>
{
    public Task<IntegrationSettingsResult> Handle(GetIntegrationSettingsQuery request, CancellationToken ct)
    {
        var showGuides = configuration.GetValue<bool>("ShowSandboxSetupGuides");
        var storageProvider = configuration.GetValue<string>("Storage:Provider") ?? "minio";
        var smtp = smtpOptions.Value;
        var minio = minioOptions.Value;
        var usps = uspsOptions.Value;
        var docuSeal = docuSealOptions.Value;
        var ollama = ollamaOptions.Value;
        var ups = upsOptions.Value;
        var fedEx = fedExOptions.Value;
        var dhl = dhlOptions.Value;
        var stamps = stampsOptions.Value;
        var xero = xeroOptions.Value;
        var freshBooks = freshBooksOptions.Value;
        var sage = sageOptions.Value;
        var netSuite = netSuiteOptions.Value;
        var wave = waveOptions.Value;
        var zoho = zohoOptions.Value;
        var localStorage = localStorageOptions.Value;

        var integrations = new List<IntegrationStatusModel>
        {
            new(
                Provider: "quickbooks",
                Name: "QuickBooks Online",
                Description: "QuickBooks Online accounting — connected via OAuth",
                Icon: "receipt_long",
                IsConfigured: false, // QB connection state is managed by AccountingService, not here
                Fields: [],
                Category: "accounting",
                LogoUrl: "https://logo.clearbit.com/quickbooks.intuit.com",
                SandboxSteps:
                [
                    "Go to developer.intuit.com and sign in with your Intuit account (or create a free developer account — no credit card required).",
                    "Click My Apps → Create an App → QuickBooks Online and Payments. Enter an app name and select the scopes you need (Accounting, and/or Payments).",
                    "In the app's Keys & OAuth section, open the Development tab. Copy the Client ID and Client Secret shown there. These are your sandbox credentials.",
                    "A sandbox company is automatically created for you — find it under Dashboard → Sandbox Companies. It comes pre-loaded with sample customers, invoices, and transactions.",
                    "Use the Development credentials here when running locally. When ready for production, switch to the Production keys (requires app review by Intuit).",
                ],
                SandboxUrl: "https://developer.intuit.com/app/developer/myapps"),
            new(
                Provider: "smtp",
                Name: "SMTP Email",
                Description: "Outbound email notifications and invoices",
                Icon: "email",
                IsConfigured: !string.IsNullOrEmpty(smtp.Host) && smtp.Host != "localhost",
                LogoUrl: null, // generic protocol — no brand logo
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
                Category: "service",
                SandboxSteps:
                [
                    "Create a free account at mailtrap.io (no credit card required). All sent emails are captured in a fake inbox — they never reach real recipients.",
                    "After login, click Email Testing in the left sidebar, then open My Inbox.",
                    "Click the inbox name, then open the SMTP Settings tab. Copy your Host (sandbox.smtp.mailtrap.io), Port (2525), Username, and Password.",
                    "Enter those credentials here. Send a test email from the app and it will appear in Mailtrap, not in anyone's real inbox.",
                ],
                SandboxUrl: "https://mailtrap.io/register/signup"),
            new(
                Provider: "minio",
                Name: "MinIO Storage",
                Description: "S3-compatible file storage for documents and attachments",
                Icon: "cloud_upload",
                IsConfigured: !string.IsNullOrEmpty(minio.Endpoint) && storageProvider == "minio",
                LogoUrl: "https://logo.clearbit.com/min.io",
                Fields:
                [
                    new("Endpoint", "Endpoint", minio.Endpoint, false, true),
                    new("AccessKey", "Access Key", minio.AccessKey, false, true),
                    new("SecretKey", "Secret Key", MaskSecret(minio.SecretKey), true, true, "password"),
                    new("UseSsl", "Use SSL", minio.UseSsl.ToString(), false, false, "toggle"),
                ],
                Category: "service",
                SandboxSteps:
                [
                    "MinIO is already running locally via Docker — no external account or sign-up is needed.",
                    "Open the MinIO console at http://localhost:9001 in your browser (default login: minioadmin / minioadmin) to browse buckets and uploaded files.",
                    "The endpoint is pre-configured. The Docker container IS the sandbox — this configuration works as-is for local development.",
                ]),
            new(
                Provider: "usps",
                Name: "USPS Address Validation",
                Description: "USPS Addresses API v3 for address verification (free with Business account)",
                Icon: "local_post_office",
                IsConfigured: !string.IsNullOrEmpty(usps.ConsumerKey),
                LogoUrl: "https://logo.clearbit.com/usps.com",
                Fields:
                [
                    new("ConsumerKey", "Consumer Key", usps.ConsumerKey, false, true),
                    new("ConsumerSecret", "Consumer Secret", MaskSecret(usps.ConsumerSecret), true, true, "password"),
                ],
                Category: "service",
                SandboxSteps:
                [
                    "Go to cop.usps.com (USPS Customer Onboarding Portal) and sign in with a USPS business account, or create one for free.",
                    "In the portal, go to My Apps and register a new application. Copy your Consumer Key and Consumer Secret from the Credentials section.",
                    "Generate an OAuth 2.0 access token via POST https://apis.usps.com/oauth2/v3/token using your Consumer Key and Secret.",
                    "For sandbox testing, use the TEM base URL: apis-tem.usps.com instead of apis.usps.com. Labels generated in TEM are watermarked and not processed for payment.",
                ],
                SandboxUrl: "https://cop.usps.com"),
            new(
                Provider: "docuseal",
                Name: "DocuSeal Document Signing",
                Description: "Electronic document signing for employee forms and contracts",
                Icon: "draw",
                IsConfigured: !string.IsNullOrEmpty(docuSeal.ApiKey),
                LogoUrl: "https://logo.clearbit.com/docuseal.com",
                Fields:
                [
                    new("BaseUrl", "Base URL", docuSeal.BaseUrl, false, true),
                    new("ApiKey", "API Key", MaskSecret(docuSeal.ApiKey), true, true, "password"),
                    new("WebhookSecret", "Webhook Secret", MaskSecret(docuSeal.WebhookSecret), true, false, "password"),
                ],
                Category: "service",
                SandboxSteps:
                [
                    "The local DocuSeal container (port 3000) is your sandbox — no external sign-up is required for self-hosted mode. Alternatively, create a free cloud account at docuseal.com/sign_up.",
                    "For cloud: after login, click your avatar (top-right) → Console → API in the left menu.",
                    "Toggle Test Mode ON in the top-right of the API page. Copy the Test Mode API Key shown — Test Mode is completely free and unlimited.",
                    "For self-hosted: open http://localhost:3000, create an admin account, then go to Settings → API to generate a key.",
                    "Use your Test Mode / self-hosted API key here. Test Mode keys cannot be used in production and vice versa.",
                ],
                SandboxUrl: "https://docuseal.com/sign_up"),
            new(
                Provider: "ollama",
                Name: "AI Assistant (Ollama)",
                Description: "Self-hosted AI for smart search, drafting, and document Q&A",
                Icon: "psychology",
                IsConfigured: !string.IsNullOrEmpty(ollama.BaseUrl),
                LogoUrl: "https://logo.clearbit.com/ollama.com",
                Fields:
                [
                    new("BaseUrl", "Base URL", ollama.BaseUrl, false, true),
                    new("Model", "Model Name", ollama.Model, false, true),
                    new("TimeoutSeconds", "Timeout (seconds)", ollama.TimeoutSeconds.ToString(), false, false, "number"),
                ],
                Category: "service",
                SandboxSteps:
                [
                    "Ollama is already running locally via Docker — no external account is needed.",
                    "Pull the default model by running: docker exec qb-engineer-ai ollama pull gemma3:4b",
                    "Pull the embedding model: docker exec qb-engineer-ai ollama pull all-minilm:l6-v2",
                    "The Base URL is pre-configured. Larger models (7B+) give better results but require more RAM.",
                ],
                SandboxUrl: "https://ollama.com/library"),
            new(
                Provider: "ups",
                Name: "UPS",
                Description: "UPS shipping rate shopping, label creation, and tracking",
                Icon: "local_shipping",
                IsConfigured: !string.IsNullOrEmpty(ups.ClientId),
                LogoUrl: "https://logo.clearbit.com/ups.com",
                Fields:
                [
                    new("ClientId", "Client ID", ups.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(ups.ClientSecret), true, true, "password"),
                    new("AccountNumber", "Account Number", ups.AccountNumber, false, false),
                    new("Environment", "Environment", ups.Environment, false, false),
                ],
                Category: "shipping",
                SandboxSteps:
                [
                    "Go to developer.ups.com and sign in with your UPS.com account (or create a free one).",
                    "Click your avatar (top-right) → Apps → Add Apps. Select 'I want to integrate UPS technology into my business', associate a UPS shipping account, and accept terms.",
                    "Add the Authorization (OAuth) product plus APIs you need (Rating, Shipping, Tracking). After creation, click the eye icon on the app to reveal your Client ID and Client Secret.",
                    "Set Environment to 'sandbox'. UPS OAuth 2.0 tokens expire every hour. Sandbox testing is free — no charges for test shipments.",
                ],
                SandboxUrl: "https://developer.ups.com/"),
            new(
                Provider: "fedex",
                Name: "FedEx",
                Description: "FedEx shipping rate shopping, label creation, and tracking",
                Icon: "local_shipping",
                IsConfigured: !string.IsNullOrEmpty(fedEx.ClientId),
                LogoUrl: "https://logo.clearbit.com/fedex.com",
                Fields:
                [
                    new("ClientId", "Client ID", fedEx.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(fedEx.ClientSecret), true, true, "password"),
                    new("AccountNumber", "Account Number", fedEx.AccountNumber, false, false),
                    new("Environment", "Environment", fedEx.Environment, false, false),
                ],
                Category: "shipping",
                SandboxSteps:
                [
                    "Register at developer.fedex.com and create a free user ID. Log in and create an Organization (required before creating any project).",
                    "Under My Projects, click Create New Project → API Project. Give it a name and select the APIs you need (Rates, Ship, Track).",
                    "Open the project and select the Test tab to retrieve your API Key (Client ID) and Secret Key (Client Secret).",
                    "Set Environment to 'sandbox'. Sandbox base URL: apis-sandbox.fedex.com (vs production: apis.fedex.com). OAuth tokens expire every hour. No real shipments or charges in sandbox.",
                ],
                SandboxUrl: "https://developer.fedex.com/"),
            new(
                Provider: "dhl",
                Name: "DHL Express",
                Description: "DHL Express international shipping",
                Icon: "flight_takeoff",
                IsConfigured: !string.IsNullOrEmpty(dhl.ApiKey),
                LogoUrl: "https://logo.clearbit.com/dhl.com",
                Fields:
                [
                    new("ApiKey", "API Key", dhl.ApiKey, false, true),
                    new("ApiSecret", "API Secret", MaskSecret(dhl.ApiSecret), true, true, "password"),
                    new("AccountNumber", "Account Number", dhl.AccountNumber, false, false),
                    new("BaseUrl", "Base URL", dhl.BaseUrl, false, false),
                ],
                Category: "shipping",
                SandboxSteps:
                [
                    "Register at developer.dhl.com/user/register with an email address and confirm via the email DHL sends you.",
                    "Log in, go to Get Access or the API Catalog, and request access to the DHL Express MyDHL API. Provide your 9-digit DHL Express account number when prompted.",
                    "Approval takes approximately 24 hours — you'll receive two emails: Test Access Approved and Production Access Approved.",
                    "Once approved, find your app in the portal dashboard and click Show Key to reveal your API Key and API Secret. Test labels are watermarked and not billed.",
                ],
                SandboxUrl: "https://developer.dhl.com/"),
            new(
                Provider: "stamps",
                Name: "Stamps.com",
                Description: "USPS postage printing and shipping label creation via Stamps.com",
                Icon: "local_post_office",
                IsConfigured: !string.IsNullOrEmpty(stamps.ApiKey),
                LogoUrl: "https://logo.clearbit.com/stamps.com",
                Fields:
                [
                    new("ApiKey", "API Key", MaskSecret(stamps.ApiKey), true, true, "password"),
                    new("AccountId", "Account ID", stamps.AccountId, false, true),
                    new("Environment", "Environment", stamps.Environment, false, false),
                ],
                Category: "shipping",
                SandboxSteps:
                [
                    "Sign up for a free developer sandbox account at developer.stamps.com. Click 'Get a Free API Key' and complete the registration form.",
                    "After registration, log into the developer portal at developer.stamps.com and create a new application to receive your Integration ID (API Key).",
                    "Set Environment to 'sandbox'. The sandbox endpoint is swsim.stamps.com. All labels printed in sandbox are watermarked 'SAMPLE' and not charged.",
                    "Your Account ID is the username you use to log in to Stamps.com. Stamps uses a SOAP-based API — test with WSDL at swsim.stamps.com/SwsimV111.asmx?WSDL.",
                ],
                SandboxUrl: "https://developer.stamps.com/"),
            new(
                Provider: "xero",
                Name: "Xero",
                Description: "Cloud accounting with multi-currency support",
                Icon: "account_balance_wallet",
                IsConfigured: !string.IsNullOrEmpty(xero.ClientId),
                LogoUrl: "https://logo.clearbit.com/xero.com",
                Fields:
                [
                    new("ClientId", "Client ID", xero.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(xero.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", xero.RedirectUri, false, false),
                ],
                Category: "accounting",
                SandboxSteps:
                [
                    "Create a free Xero account at xero.com/signup, then log in to developer.xero.com → My Apps → New App.",
                    "Enter app name, company URL, and your Redirect URI (e.g. http://localhost:5000/api/v1/accounting/xero/callback). Copy the Client ID shown immediately.",
                    "Click Generate a secret to create your Client Secret — copy it immediately, it is shown only once.",
                    "For a test org: log in to Xero → click your org name → My Xero → Try the Demo Company. When connecting via OAuth, select the Demo Company instead of a real org.",
                ],
                SandboxUrl: "https://developer.xero.com/app/manage"),
            new(
                Provider: "freshbooks",
                Name: "FreshBooks",
                Description: "Small business invoicing and accounting",
                Icon: "receipt_long",
                IsConfigured: !string.IsNullOrEmpty(freshBooks.ClientId),
                LogoUrl: "https://logo.clearbit.com/freshbooks.com",
                Fields:
                [
                    new("ClientId", "Client ID", freshBooks.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(freshBooks.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", freshBooks.RedirectUri, false, false),
                ],
                Category: "accounting",
                SandboxSteps:
                [
                    "Create a free FreshBooks account at freshbooks.com/signup, then go to my.freshbooks.com/#/developer.",
                    "Click Create App. Enter app name, description, website URL, and Redirect URI (HTTPS required; self-signed certs work for localhost).",
                    "Copy your Client ID and Client Secret from the app settings. Scroll down to find the pre-built Authorization URL to start the OAuth flow.",
                    "Your own FreshBooks account doubles as the sandbox — no separate environment exists. All API calls use api.freshbooks.com.",
                ],
                SandboxUrl: "https://my.freshbooks.com/#/developer"),
            new(
                Provider: "sage",
                Name: "Sage Business Cloud",
                Description: "Sage Business Cloud Accounting",
                Icon: "business",
                IsConfigured: !string.IsNullOrEmpty(sage.ClientId),
                LogoUrl: "https://logo.clearbit.com/sage.com",
                Fields:
                [
                    new("ClientId", "Client ID", sage.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(sage.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", sage.RedirectUri, false, false),
                    new("CountryCode", "Country Code", sage.CountryCode, false, false),
                ],
                Category: "accounting",
                SandboxSteps:
                [
                    "Create a free developer account at developer.sage.com, then navigate to My Applications → register a new application.",
                    "Enter app name, description, and your Redirect URI. When submitting, request Development credentials (not production) to receive sandbox-scoped keys.",
                    "Credential provisioning can take up to 72 hours. Once approved, copy your Client ID and Client Secret from the developer console. The sandbox includes sample companies, contacts, invoices, and transactions.",
                    "Set Country Code to your region (e.g. 'US', 'GB', 'CA'). Sage Accounting and Sage 200 have separate APIs — register separately if you need both.",
                ],
                SandboxUrl: "https://developer.sage.com/accounting/"),
            new(
                Provider: "netsuite",
                Name: "NetSuite",
                Description: "NetSuite ERP (Token-Based Authentication)",
                Icon: "corporate_fare",
                IsConfigured: !string.IsNullOrEmpty(netSuite.AccountId),
                LogoUrl: "https://logo.clearbit.com/netsuite.com",
                Fields:
                [
                    new("AccountId", "Account ID", netSuite.AccountId, false, true),
                    new("ConsumerKey", "Consumer Key", netSuite.ConsumerKey, false, true),
                    new("ConsumerSecret", "Consumer Secret", MaskSecret(netSuite.ConsumerSecret), true, true, "password"),
                    new("TokenId", "Token ID", netSuite.TokenId, false, true),
                    new("TokenSecret", "Token Secret", MaskSecret(netSuite.TokenSecret), true, true, "password"),
                ],
                Category: "accounting",
                SandboxSteps:
                [
                    "Start with a NetSuite Free Trial at netsuite.com/portal/free-trial.shtml for a usable dev account. Existing customers request a sandbox via Support → Manage Support Cases (sandbox URL format: {accountId}-sb1.app.netsuite.com).",
                    "Enable SuiteTalk REST Web Services: Setup → Company → Enable Features → SuiteCloud tab → check 'Token-Based Authentication' and 'REST Web Services'.",
                    "Create an Integration Record: Setup → Integration → Manage Integrations → New. This generates your Consumer Key and Consumer Secret — copy them immediately.",
                    "Create an Access Token: Setup → Users/Roles → Access Tokens → New. Select your integration record and user, then copy the Token ID and Token Secret — shown only once.",
                    "Your Account ID is visible in the URL when logged in (e.g., TSTDRV123456 for sandbox; note: OAuth realm uses underscores, e.g. 123456_SB1).",
                ],
                SandboxUrl: "https://www.netsuite.com/portal/free-trial.shtml"),
            new(
                Provider: "wave",
                Name: "Wave",
                Description: "Free small business accounting",
                Icon: "waves",
                IsConfigured: !string.IsNullOrEmpty(wave.AccessToken),
                LogoUrl: "https://logo.clearbit.com/waveapps.com",
                Fields:
                [
                    new("AccessToken", "Access Token", MaskSecret(wave.AccessToken), true, true, "password"),
                    new("BusinessId", "Business ID", wave.BusinessId, false, true),
                ],
                Category: "accounting",
                SandboxSteps:
                [
                    "Create a free Wave account at waveapps.com. Log in, then go to developer.waveapps.com → Manage Applications → Create Application.",
                    "Enter app name, description, and Redirect URI (HTTPS; https://localhost is acceptable for dev). Copy your Client ID and Client Secret.",
                    "Initiate OAuth via POST to api.waveapps.com/oauth2/authorize/ with your Client ID. Wave uses a GraphQL API (single endpoint: gql.waveapps.com/graphql/public).",
                    "Your Business ID appears in the URL when viewing your Wave business dashboard. Note: as of May 2025, end-users connecting via API need a Wave Pro Plan — developers can use Full Access Tokens for personal testing without a subscription.",
                ],
                SandboxUrl: "https://developer.waveapps.com"),
            new(
                Provider: "zoho",
                Name: "Zoho Books",
                Description: "Zoho Books accounting and invoicing",
                Icon: "menu_book",
                IsConfigured: !string.IsNullOrEmpty(zoho.ClientId),
                LogoUrl: "https://logo.clearbit.com/zoho.com",
                Fields:
                [
                    new("ClientId", "Client ID", zoho.ClientId, false, true),
                    new("ClientSecret", "Client Secret", MaskSecret(zoho.ClientSecret), true, true, "password"),
                    new("RedirectUri", "Redirect URI", zoho.RedirectUri, false, false),
                    new("OrganizationId", "Organization ID", zoho.OrganizationId, false, true),
                    new("DataCenter", "Data Center", zoho.DataCenter, false, false),
                ],
                Category: "accounting",
                SandboxSteps:
                [
                    "Create a free Zoho account at zoho.com/signup, then go to accounts.zoho.com/developerconsole → Add Client ID → Server-Based Applications.",
                    "Enter app name, website URL, and Redirect URI (e.g. http://localhost:5000/api/v1/accounting/zoho/callback). Copy your Client ID and Client Secret.",
                    "For a sandbox org: in Zoho Books go to Settings → Developer Space → Sandbox to create an isolated sandbox organization. Sandbox refresh tokens cannot be used in production orgs.",
                    "Your Organization ID is shown in Manage Organizations in Zoho Books, or via GET /organizations. Data Center must match where your org was created: 'com' (US), 'eu' (EU), 'in' (India), 'com.au' (AU), 'jp' (Japan).",
                ],
                SandboxUrl: "https://api-console.zoho.com/"),
            new(
                Provider: "local-storage",
                Name: "Local File Storage",
                Description: "Serve files from the host filesystem — no external service required",
                Icon: "folder",
                IsConfigured: storageProvider == "local",
                LogoUrl: null,
                Fields:
                [
                    new("RootPath", "Root Path", localStorage.RootPath, false, true),
                    new("PublicBaseUrl", "Public Base URL", localStorage.PublicBaseUrl, false, true),
                ],
                Category: "service",
                SandboxSteps:
                [
                    "Set the STORAGE_PROVIDER environment variable to 'local' (or set Storage:Provider=local in appsettings.json).",
                    "Set LOCAL_STORAGE_PATH in your .env file to the host directory you want to use (e.g. LOCAL_STORAGE_PATH=./storage). This mounts the directory into the container at /app/storage.",
                    "Set LOCAL_STORAGE_PUBLIC_URL to the public base URL the API is reachable at (e.g. http://localhost:5000). This is used to construct presigned download URLs.",
                    "Files are stored at {RootPath}/{bucket}/{key} inside the container. Presigned URLs use ASP.NET Data Protection tokens and expire after the configured expiry period.",
                    "Restart the API container to apply storage provider changes: docker compose up -d --build qb-engineer-api",
                ]),
        };

        return Task.FromResult(new IntegrationSettingsResult(showGuides, integrations));
    }

    private static string MaskSecret(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Length <= 4) return new string('*', value.Length);
        return new string('*', value.Length - 4) + value[^4..];
    }
}
