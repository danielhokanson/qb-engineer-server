using QBEngineer.Core.Entities;

namespace QBEngineer.Core.Interfaces;

public interface ITrainingVideoGeneratorService
{
    /// <summary>
    /// Captures one screenshot per walkthrough step (element highlighted),
    /// pairs each with TTS narration, and returns the final MP4 as a byte array.
    /// </summary>
    Task<byte[]> GenerateVideoAsync(TrainingModule module, string jwtToken, CancellationToken ct = default);
}
