using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

/// <summary>
/// TTS implementation backed by a self-hosted Coqui TTS HTTP server.
/// The server returns WAV audio; this service converts to MP3 via ffmpeg
/// so the output format is consistent with OpenAiTtsService.
/// </summary>
public class CoquiTtsService(
    IHttpClientFactory httpClientFactory,
    IOptions<CoquiOptions> options,
    ILogger<CoquiTtsService> logger) : ITtsService
{
    private readonly CoquiOptions _opts = options.Value;

    public async Task<byte[]> GenerateSpeechAsync(string text, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("coqui-tts");

        var url = $"{_opts.BaseUrl.TrimEnd('/')}/api/tts?text={Uri.EscapeDataString(text)}";
        if (!string.IsNullOrWhiteSpace(_opts.SpeakerId))
            url += $"&speaker_id={Uri.EscapeDataString(_opts.SpeakerId)}";

        logger.LogDebug("Coqui TTS: generating {Chars} chars", text.Length);

        var response = await client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        // Coqui returns WAV — normalise to MP3 so callers get a consistent format
        var wavBytes = await response.Content.ReadAsByteArrayAsync(ct);
        return await ConvertWavToMp3Async(wavBytes, ct);
    }

    private static async Task<byte[]> ConvertWavToMp3Async(byte[] wav, CancellationToken ct)
    {
        var tmpWav = Path.ChangeExtension(Path.GetTempFileName(), ".wav");
        var tmpMp3 = Path.ChangeExtension(Path.GetTempFileName(), ".mp3");

        try
        {
            await File.WriteAllBytesAsync(tmpWav, wav, ct);

            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = "ffmpeg",
                    Arguments              = $"-y -i \"{tmpWav}\" -c:a libmp3lame -b:a 192k \"{tmpMp3}\"",
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                },
            };

            proc.Start();
            await proc.WaitForExitAsync(ct);

            return await File.ReadAllBytesAsync(tmpMp3, ct);
        }
        finally
        {
            try { File.Delete(tmpWav); } catch { /* best-effort */ }
            try { File.Delete(tmpMp3); } catch { /* best-effort */ }
        }
    }
}
