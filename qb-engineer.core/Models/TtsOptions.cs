namespace QBEngineer.Core.Models;

public class TtsOptions
{
    public string ApiKey  { get; set; } = string.Empty;
    public string Model   { get; set; } = "tts-1";
    public string Voice   { get; set; } = "nova";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}
