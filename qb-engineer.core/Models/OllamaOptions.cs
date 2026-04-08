namespace QBEngineer.Core.Models;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://qb-engineer-ai:11434";
    public string Model { get; set; } = "gemma3:4b";
    public string EmbeddingModel { get; set; } = "all-minilm:l6-v2";
    public string VisionModel { get; set; } = "llava:7b";
    public int TimeoutSeconds { get; set; } = 120;
    public int VisionTimeoutSeconds { get; set; } = 600;
    public string DocsPath { get; set; } = "/app/docs";
}
