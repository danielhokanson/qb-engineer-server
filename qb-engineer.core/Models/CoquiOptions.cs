namespace QBEngineer.Core.Models;

public class CoquiOptions
{
    public const string SectionName = "Coqui";

    /// <summary>Base URL of the Coqui TTS HTTP server, e.g. http://qb-engineer-tts:5002</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Speaker ID for multi-speaker models (VCTK: p267, p273, etc.).</summary>
    public string SpeakerId { get; set; } = string.Empty;
}
