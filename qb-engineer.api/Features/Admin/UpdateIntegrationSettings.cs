using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Admin;

public record UpdateIntegrationSettingsCommand(
    string Provider,
    Dictionary<string, string> Settings) : IRequest<IntegrationStatusModel>;

public class UpdateIntegrationSettingsHandler(
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
    IOptions<LocalStorageOptions> localStorageOptions,
    ISystemSettingRepository settingRepo) : IRequestHandler<UpdateIntegrationSettingsCommand, IntegrationStatusModel>
{
    public async Task<IntegrationStatusModel> Handle(UpdateIntegrationSettingsCommand request, CancellationToken ct)
    {
        // Persist settings to system_settings table AND update in-memory IOptions
        foreach (var (key, value) in request.Settings)
        {
            // Skip masked values (unchanged secrets)
            if (IsAllMasked(value)) continue;

            var settingKey = $"integration.{request.Provider}.{key}";
            await settingRepo.UpsertAsync(settingKey, value, $"{request.Provider} integration setting: {key}", ct);
        }
        await settingRepo.SaveChangesAsync(ct);

        // Update in-memory options so services pick up changes without restart
        switch (request.Provider)
        {
            case "smtp":
                ApplySmtpSettings(request.Settings);
                break;
            case "minio":
                ApplyMinioSettings(request.Settings);
                break;
            case "usps":
                ApplyUspsSettings(request.Settings);
                break;
            case "docuseal":
                ApplyDocuSealSettings(request.Settings);
                break;
            case "ollama":
                ApplyOllamaSettings(request.Settings);
                break;
            case "local-storage":
                ApplyLocalStorageSettings(request.Settings);
                break;
            default:
                // Carrier and OAuth-based accounting providers (ups, fedex, dhl, xero, freshbooks,
                // sage, netsuite, wave, zoho) are configured via appsettings / environment variables,
                // not via in-memory mutation. Settings are persisted to system_settings above; a
                // restart picks them up from IOptions. No in-memory update needed here.
                break;
        }

        // Return updated status via GetIntegrationSettings (re-read from updated options)
        var handler = new GetIntegrationSettingsHandler(configuration, smtpOptions, minioOptions, uspsOptions, docuSealOptions, ollamaOptions,
            upsOptions, fedExOptions, dhlOptions, stampsOptions, xeroOptions, freshBooksOptions, sageOptions, netSuiteOptions, waveOptions, zohoOptions, localStorageOptions);
        var result = await handler.Handle(new GetIntegrationSettingsQuery(), ct);
        return result.Integrations.First(i => i.Provider == request.Provider);
    }

    private void ApplySmtpSettings(Dictionary<string, string> settings)
    {
        var opts = smtpOptions.Value;
        if (settings.TryGetValue("Host", out var host)) opts.Host = host;
        if (settings.TryGetValue("Port", out var port) && int.TryParse(port, out var p)) opts.Port = p;
        if (settings.TryGetValue("Username", out var user)) opts.Username = user;
        if (settings.TryGetValue("Password", out var pass) && !IsAllMasked(pass)) opts.Password = pass;
        if (settings.TryGetValue("UseSsl", out var ssl) && bool.TryParse(ssl, out var s)) opts.UseSsl = s;
        if (settings.TryGetValue("FromAddress", out var from)) opts.FromAddress = from;
        if (settings.TryGetValue("FromName", out var name)) opts.FromName = name;
    }

    private void ApplyMinioSettings(Dictionary<string, string> settings)
    {
        var opts = minioOptions.Value;
        if (settings.TryGetValue("Endpoint", out var ep)) opts.Endpoint = ep;
        if (settings.TryGetValue("AccessKey", out var ak)) opts.AccessKey = ak;
        if (settings.TryGetValue("SecretKey", out var sk) && !IsAllMasked(sk)) opts.SecretKey = sk;
        if (settings.TryGetValue("UseSsl", out var ssl) && bool.TryParse(ssl, out var s)) opts.UseSsl = s;
    }

    private void ApplyUspsSettings(Dictionary<string, string> settings)
    {
        var opts = uspsOptions.Value;
        if (settings.TryGetValue("ConsumerKey", out var ck)) opts.ConsumerKey = ck;
        if (settings.TryGetValue("ConsumerSecret", out var cs) && !IsAllMasked(cs)) opts.ConsumerSecret = cs;
    }

    private void ApplyDocuSealSettings(Dictionary<string, string> settings)
    {
        var opts = docuSealOptions.Value;
        if (settings.TryGetValue("BaseUrl", out var url)) opts.BaseUrl = url;
        if (settings.TryGetValue("ApiKey", out var key) && !IsAllMasked(key)) opts.ApiKey = key;
        if (settings.TryGetValue("WebhookSecret", out var ws) && !IsAllMasked(ws)) opts.WebhookSecret = ws;
    }

    private void ApplyOllamaSettings(Dictionary<string, string> settings)
    {
        var opts = ollamaOptions.Value;
        if (settings.TryGetValue("BaseUrl", out var url)) opts.BaseUrl = url;
        if (settings.TryGetValue("Model", out var model)) opts.Model = model;
        if (settings.TryGetValue("TimeoutSeconds", out var timeout) && int.TryParse(timeout, out var t)) opts.TimeoutSeconds = t;
    }

    private void ApplyLocalStorageSettings(Dictionary<string, string> settings)
    {
        var opts = localStorageOptions.Value;
        if (settings.TryGetValue("RootPath", out var root)) opts.RootPath = root;
        if (settings.TryGetValue("PublicBaseUrl", out var url)) opts.PublicBaseUrl = url;
    }

    private static bool IsAllMasked(string value) => !string.IsNullOrEmpty(value) && value.All(c => c == '*');
}
