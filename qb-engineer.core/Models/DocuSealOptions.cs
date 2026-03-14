namespace QBEngineer.Core.Models;

public class DocuSealOptions
{
    public const string SectionName = "DocuSeal";

    public string BaseUrl { get; set; } = "http://qb-engineer-signing:3000";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string WebhookSecret { get; set; } = string.Empty;
}
