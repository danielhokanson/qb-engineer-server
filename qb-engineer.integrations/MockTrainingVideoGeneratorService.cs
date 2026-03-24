using Microsoft.Extensions.Logging;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Integrations;

/// <summary>
/// Mock — returns a minimal parseable MP4 stub without Puppeteer, TTS, or ffmpeg.
/// </summary>
public class MockTrainingVideoGeneratorService(
    ILogger<MockTrainingVideoGeneratorService> logger) : ITrainingVideoGeneratorService
{
    // Minimal ISO Base Media File Format (ftyp + empty mdat)
    private static readonly byte[] PlaceholderMp4 =
    [
        0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70,
        0x69, 0x73, 0x6F, 0x6D, 0x00, 0x00, 0x00, 0x00,
        0x69, 0x73, 0x6F, 0x6D, 0x6D, 0x70, 0x34, 0x31,
        0x00, 0x00, 0x00, 0x08, 0x6D, 0x64, 0x61, 0x74,
    ];

    public Task<byte[]> GenerateVideoAsync(
        TrainingModule module,
        string jwtToken,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "MockVideoGenerator: placeholder MP4 for module {Id} '{Title}'",
            module.Id, module.Title);

        return Task.FromResult(PlaceholderMp4);
    }
}
