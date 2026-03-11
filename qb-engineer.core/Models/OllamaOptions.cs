namespace QBEngineer.Core.Models;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://qb-engineer-ai:11434";
    public string Model { get; set; } = "llama3.2:3b";
    public int TimeoutSeconds { get; set; } = 120;
}
