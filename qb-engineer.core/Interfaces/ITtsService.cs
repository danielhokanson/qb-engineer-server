namespace QBEngineer.Core.Interfaces;

public interface ITtsService
{
    /// <summary>Returns MP3 audio bytes for the given text.</summary>
    Task<byte[]> GenerateSpeechAsync(string text, CancellationToken ct = default);
}
