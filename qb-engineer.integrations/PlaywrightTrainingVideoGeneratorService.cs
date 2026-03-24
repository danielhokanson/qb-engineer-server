using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Integrations;

/// <summary>
/// Generates training video tutorials by combining:
///   1. Playwright live browser recording (real screen capture of the running app)
///   2. TTS narration per walkthrough step (OpenAI or Coqui)
///   3. ffmpeg mux — narration audio is synchronized to step highlights via precise timing
///
/// Sync strategy: TTS clips are generated first and their durations measured via ffprobe.
/// Playwright holds on each highlighted step for exactly that clip's duration.
/// A silence clip equal to the page load+settle time is prepended to the audio track
/// so narration begins the moment the first element highlight appears on screen.
/// </summary>
public class PlaywrightTrainingVideoGeneratorService(
    ITtsService tts,
    IConfiguration configuration,
    ILogger<PlaywrightTrainingVideoGeneratorService> logger) : ITrainingVideoGeneratorService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ── Public API ─────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateVideoAsync(
        TrainingModule module,
        string jwtToken,
        CancellationToken ct = default)
    {
        var content = JsonSerializer.Deserialize<WalkthroughContent>(module.ContentJson ?? "{}", JsonOpts);
        if (content?.Steps is not { Count: > 0 })
            throw new InvalidOperationException("Module has no walkthrough steps.");

        var appRoute = content.AppRoute ?? "/dashboard";
        var baseUrl  = GetBaseUrl();
        var tokenKey = configuration["Auth:LocalStorageTokenKey"] ?? "qbe-token";
        var workDir  = Path.Combine(Path.GetTempPath(), $"qbe-video-{module.Id}-{Guid.NewGuid():N}");
        var videoDir = Path.Combine(workDir, "video");
        Directory.CreateDirectory(workDir);
        Directory.CreateDirectory(videoDir);

        try
        {
            // 1. Pre-generate TTS for every step + measure each clip's duration
            logger.LogInformation("VideoGen: generating TTS for {Count} steps", content.Steps.Count);
            var audioPaths = await GenerateAudioClipsAsync(content.Steps, workDir, ct);
            var holdMs     = await BuildHoldTimingsAsync(audioPaths, content.Steps, ct);

            // 2. Record live browser session — Playwright captures everything including the
            //    page loading phase; returns the WebM path and how long loading took
            logger.LogInformation("VideoGen: starting Playwright recording — {Steps} steps", content.Steps.Count);
            var (webmPath, navMs) = await RecordBrowserSessionAsync(
                baseUrl, appRoute, tokenKey, jwtToken,
                content.Steps, holdMs, videoDir, ct);

            // 3. Prepend silence == nav time so audio starts in sync with the first highlight
            var narrationPath = Path.Combine(workDir, "narration.mp3");
            await BuildNarrationTrackAsync(audioPaths, navMs, narrationPath, ct);

            // 4. Re-encode WebM + add narration → final MP4
            var outputPath = Path.Combine(workDir, "output.mp4");
            await MuxVideoAudioAsync(webmPath, narrationPath, outputPath, ct);

            var size = new FileInfo(outputPath).Length;
            logger.LogInformation("VideoGen: complete — {Kb} KB", size / 1024);
            return await File.ReadAllBytesAsync(outputPath, ct);
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }

    // ── Step 1: TTS generation ─────────────────────────────────────────────

    private async Task<List<string>> GenerateAudioClipsAsync(
        List<WalkthroughStep> steps, string workDir, CancellationToken ct)
    {
        var paths = new List<string>();
        for (var i = 0; i < steps.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var text  = BuildNarration(steps[i]);
            var bytes = await tts.GenerateSpeechAsync(text, ct);
            var path  = Path.Combine(workDir, $"audio_{i}.mp3");
            await File.WriteAllBytesAsync(path, bytes, ct);
            paths.Add(path);
            logger.LogDebug("VideoGen: TTS {Index}/{Total}", i + 1, steps.Count);
        }
        return paths;
    }

    private static string BuildNarration(WalkthroughStep step)
    {
        var title = step.Popover.Title.TrimEnd('.');
        var desc  = step.Popover.Description?.TrimEnd('.') ?? string.Empty;
        return string.IsNullOrWhiteSpace(desc) ? $"{title}." : $"{title}. {desc}.";
    }

    /// <summary>
    /// Returns per-step hold times in milliseconds.
    /// Uses the actual TTS audio duration when available (OpenAI / Coqui).
    /// Falls back to estimated speaking time from word count (130 wpm) when mock TTS
    /// returns a near-zero duration — ensures slides are still legibly timed.
    /// Also replaces the invalid mock MP3 stub with a proper ffmpeg-generated silence clip
    /// at the estimated duration so the narration concat step never receives a corrupt file.
    /// </summary>
    private async Task<List<int>> BuildHoldTimingsAsync(
        List<string> audioPaths, List<WalkthroughStep> steps, CancellationToken ct)
    {
        var timings = new List<int>();
        for (var i = 0; i < audioPaths.Count; i++)
        {
            var durationSec = await GetAudioDurationAsync(audioPaths[i], ct);

            // When TTS is mocked it returns a single silent frame (~0.026 s).
            // Estimate reading time from word count so the recording is still usable.
            if (durationSec < 0.5)
            {
                var text      = BuildNarration(steps[i]);
                var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                durationSec   = Math.Max(3.0, wordCount / 130.0 * 60.0); // 130 wpm, floor 3 s

                // Replace the invalid mock stub with a proper silence clip at the estimated
                // duration so ffmpeg can concat it without "Invalid frame size" errors.
                var silenceSecs = durationSec.ToString("F3", CultureInfo.InvariantCulture);
                await RunFfmpegAsync(
                    $"-y -f lavfi -i anullsrc=r=44100:cl=stereo -t {silenceSecs} -c:a libmp3lame -b:a 128k \"{audioPaths[i]}\"",
                    ct);
            }

            timings.Add((int)((durationSec + 0.5) * 1000)); // 500 ms tail pause
        }
        return timings;
    }

    private async Task<double> GetAudioDurationAsync(string path, CancellationToken ct)
    {
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "ffprobe",
                Arguments              = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            },
        };
        proc.Start();
        var output = await proc.StandardOutput.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        // Return 0.0 when ffprobe fails (exit code != 0) or produces no parseable output.
        // 0.0 triggers the mock-TTS replacement branch in BuildHoldTimingsAsync.
        // A real 3-second file returns a positive duration; the 3.0 fallback was masking
        // the 60-byte mock stub (which ffprobe rejects with "Invalid frame size").
        if (proc.ExitCode != 0 || !double.TryParse(output.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return 0.0;
        return d;
    }

    // ── Step 2: Playwright browser recording ──────────────────────────────

    private async Task<(string WebmPath, int NavMs)> RecordBrowserSessionAsync(
        string baseUrl,
        string appRoute,
        string tokenKey,
        string jwtToken,
        List<WalkthroughStep> steps,
        List<int> holdMs,
        string videoDir,
        CancellationToken ct)
    {
        var execPath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

        using var playwright = await Playwright.CreateAsync();

        var launchOpts = new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args     = ["--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage", "--disable-setuid-sandbox"],
        };
        if (!string.IsNullOrWhiteSpace(execPath))
            launchOpts.ExecutablePath = execPath;

        await using var browser = await playwright.Chromium.LaunchAsync(launchOpts);

        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize    = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir  = videoDir,
            RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 },
        });

        var page = await context.NewPageAsync();

        // Inject JWT into localStorage before Angular boots
        var baseOnly = baseUrl.TrimEnd('/');
        await page.GotoAsync(baseOnly);
        await page.EvaluateAsync($"() => localStorage.setItem('{tokenKey}', '{jwtToken}')");

        // Navigate to the target page — measure elapsed time for audio sync
        var navSw = Stopwatch.StartNew();
        await page.GotoAsync($"{baseOnly}{appRoute}", new PageGotoOptions
        {
            Timeout   = 45_000,
            WaitUntil = WaitUntilState.NetworkIdle,
        });
        await page.WaitForSelectorAsync("app-root", new PageWaitForSelectorOptions { Timeout = 10_000 });
        await page.WaitForTimeoutAsync(2_000); // Angular change-detection settle
        navSw.Stop();
        var navMs = (int)navSw.ElapsedMilliseconds;
        logger.LogInformation("VideoGen: page ready in {Ms}ms", navMs);

        // Animate each walkthrough step — hold duration matches TTS clip length
        for (var i = 0; i < steps.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var step = steps[i];

            if (!string.IsNullOrWhiteSpace(step.Element))
            {
                await HighlightElementAsync(page, step.Element);
                await page.WaitForTimeoutAsync(150); // let highlight render in video
            }

            await page.WaitForTimeoutAsync(holdMs[i]);

            if (!string.IsNullOrWhiteSpace(step.Element))
                await RemoveHighlightAsync(page);

            await page.WaitForTimeoutAsync(200); // brief gap between steps
            logger.LogDebug("VideoGen: recorded step {Index}/{Total} ({Hold}ms)", i + 1, steps.Count, holdMs[i]);
        }

        await page.WaitForTimeoutAsync(800); // short tail before fade-out

        // Retrieve the video path BEFORE closing context, then close to finalize WebM
        var videoPath = await page.Video!.PathAsync();
        await context.CloseAsync();

        return (videoPath, navMs);
    }

    private static Task HighlightElementAsync(IPage page, string selector)
    {
        var escaped = selector.Replace("\\", "\\\\").Replace("'", "\\'");
        return page.EvaluateAsync($$"""
            () => {
                const el = document.querySelector('{{escaped}}');
                if (!el) return;
                el.scrollIntoView({ behavior: 'instant', block: 'center' });
                const r = el.getBoundingClientRect();
                const ov = document.createElement('div');
                ov.id = '__qbe_hl__';
                ov.style.cssText = [
                    'position:fixed',
                    `top:${r.top - 6}px`,
                    `left:${r.left - 6}px`,
                    `width:${r.width + 12}px`,
                    `height:${r.height + 12}px`,
                    'border:3px solid #FF6B00',
                    'border-radius:4px',
                    'box-shadow:0 0 0 9999px rgba(0,0,0,0.45)',
                    'z-index:2147483647',
                    'pointer-events:none',
                ].join(';');
                document.body.appendChild(ov);
            }
        """);
    }

    private static Task RemoveHighlightAsync(IPage page) =>
        page.EvaluateAsync("() => { const el = document.getElementById('__qbe_hl__'); if (el) el.remove(); }");

    // ── Step 3: Build narration audio track ──────────────────────────────

    private async Task BuildNarrationTrackAsync(
        List<string> audioPaths, int navMs, string outputPath, CancellationToken ct)
    {
        // Generate a silence clip that covers the page load+settle period
        var silenceSecs = (navMs / 1000.0).ToString("F3", CultureInfo.InvariantCulture);
        var silencePath = Path.Combine(Path.GetDirectoryName(outputPath)!, "silence.mp3");
        await RunFfmpegAsync(
            $"-y -f lavfi -i anullsrc=r=44100:cl=stereo -t {silenceSecs} -c:a libmp3lame -b:a 128k \"{silencePath}\"",
            ct);

        var parts = new[] { silencePath }.Concat(audioPaths).ToList();

        if (parts.Count == 1)
        {
            File.Copy(parts[0], outputPath, overwrite: true);
            return;
        }

        // Concatenate: [silence] + [step0] + [step1] + ...
        var inputs  = string.Join(' ', parts.Select(p => $"-i \"{p}\""));
        var labels  = string.Concat(Enumerable.Range(0, parts.Count).Select(i => $"[{i}:a]"));
        var filter  = $"{labels}concat=n={parts.Count}:v=0:a=1[aout]";
        await RunFfmpegAsync(
            $"-y {inputs} -filter_complex \"{filter}\" -map [aout] -c:a libmp3lame -b:a 192k \"{outputPath}\"",
            ct);
    }

    // ── Step 4: Mux video + narration → MP4 ──────────────────────────────

    private async Task MuxVideoAudioAsync(
        string webmPath, string audioPath, string outputPath, CancellationToken ct)
    {
        // Re-encode VP8/VP9 WebM to H.264 and merge with narration audio.
        // -map 0:v:0 / -map 1:a:0 ensures we take video from WebM and audio from MP3,
        // discarding the silent audio track Playwright records alongside the video.
        var args = string.Join(' ',
            "-y",
            $"-i \"{webmPath}\"",
            $"-i \"{audioPath}\"",
            "-c:v libx264 -preset fast -crf 23 -pix_fmt yuv420p",
            "-c:a aac -b:a 192k",
            "-map 0:v:0 -map 1:a:0",
            $"\"{outputPath}\"");
        await RunFfmpegAsync(args, ct);
    }

    // ── ffmpeg / ffprobe helper ───────────────────────────────────────────

    private async Task RunFfmpegAsync(string args, CancellationToken ct)
    {
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "ffmpeg",
                Arguments              = args,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            },
        };
        proc.Start();
        var stderr = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        if (proc.ExitCode != 0)
        {
            // Trim version header so the actual error is visible in logs.
            // The version block ends around "libpostproc" line; find it and skip past.
            var relevantStart = stderr.IndexOf("libpostproc", StringComparison.Ordinal);
            var relevant = relevantStart >= 0 ? stderr[(relevantStart + 60)..] : stderr;
            var snippet = relevant.Trim()[..Math.Min(relevant.Trim().Length, 1000)];
            logger.LogError("ffmpeg error: {Error}", snippet);
            throw new InvalidOperationException($"ffmpeg failed (exit {proc.ExitCode}): {snippet}");
        }
    }

    private string GetBaseUrl()
    {
        var url = configuration["FormRendererBaseUrl"] ?? configuration["FrontendBaseUrl"];
        return string.IsNullOrWhiteSpace(url) ? "http://localhost:4200" : url;
    }

    // ── Internal DTOs ─────────────────────────────────────────────────────

    // Video modules store steps under the same schema as Walkthrough modules —
    // the generator reads appRoute + steps regardless of the parent content type.
    private record WalkthroughContent(
        [property: JsonPropertyName("appRoute")]         string? AppRoute,
        [property: JsonPropertyName("startButtonLabel")] string? StartButtonLabel,
        [property: JsonPropertyName("steps")]            List<WalkthroughStep> Steps);

    private record WalkthroughStep(
        [property: JsonPropertyName("element")]  string? Element,
        [property: JsonPropertyName("popover")]  WalkthroughPopoverDto Popover);

    private record WalkthroughPopoverDto(
        [property: JsonPropertyName("title")]       string Title,
        [property: JsonPropertyName("description")] string? Description);
}
