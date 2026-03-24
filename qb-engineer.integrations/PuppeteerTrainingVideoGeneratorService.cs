using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using PuppeteerSharp;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Integrations;

public class PuppeteerTrainingVideoGeneratorService(
    ITtsService tts,
    IConfiguration configuration,
    ILogger<PuppeteerTrainingVideoGeneratorService> logger) : ITrainingVideoGeneratorService, IAsyncDisposable
{
    private IBrowser? _browser;
    private readonly SemaphoreSlim _browserLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ──────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────

    public async Task<byte[]> GenerateVideoAsync(
        TrainingModule module,
        string jwtToken,
        CancellationToken ct = default)
    {
        var content = JsonSerializer.Deserialize<WalkthroughContent>(module.ContentJson ?? "{}", JsonOpts);
        if (content?.Steps is not { Count: > 0 })
            throw new InvalidOperationException("Module has no walkthrough steps.");

        var appRoute   = content.AppRoute ?? "/dashboard";
        var baseUrl    = GetBaseUrl();
        var tokenKey   = configuration["Auth:LocalStorageTokenKey"] ?? "qbe-token";
        var workDir    = Path.Combine(Path.GetTempPath(), $"qbe-video-{module.Id}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workDir);

        try
        {
            // 1. Capture one screenshot per step
            var screenshots = await CaptureStepsAsync(
                baseUrl, appRoute, tokenKey, jwtToken, content.Steps, workDir, ct);

            // 2. Generate TTS audio for each step
            await GenerateAudioAsync(content.Steps, workDir, ct);

            // 3. Build per-step MP4 segments
            await BuildSegmentsAsync(screenshots.Count, workDir, ct);

            // 4. Concatenate into final MP4
            var outputPath = Path.Combine(workDir, "output.mp4");
            await ConcatenateSegmentsAsync(screenshots.Count, workDir, outputPath, ct);

            return await File.ReadAllBytesAsync(outputPath, ct);
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); }
            catch { /* best effort cleanup */ }
        }
    }

    // ──────────────────────────────────────────────────────────
    // Step 1 — Puppeteer screenshots
    // ──────────────────────────────────────────────────────────

    private async Task<List<string>> CaptureStepsAsync(
        string baseUrl,
        string appRoute,
        string tokenKey,
        string jwtToken,
        List<WalkthroughStep> steps,
        string workDir,
        CancellationToken ct)
    {
        var browser = await GetBrowserAsync();
        var page    = await browser.NewPageAsync();
        var paths   = new List<string>();

        try
        {
            await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });

            // Navigate to app root first so localStorage is in scope, inject JWT, then navigate to target
            var baseOnly = baseUrl.TrimEnd('/');
            await page.GoToAsync(baseOnly, new NavigationOptions { Timeout = 30_000, WaitUntil = [WaitUntilNavigation.DOMContentLoaded] });
            await page.EvaluateFunctionAsync($"() => localStorage.setItem('{tokenKey}', '{jwtToken}')");

            var url = $"{baseOnly}{appRoute}";
            logger.LogInformation("VideoGen: navigating to {Url}", url);
            await page.GoToAsync(url, new NavigationOptions
            {
                Timeout = 45_000,
                WaitUntil = [WaitUntilNavigation.Networkidle2],
            });

            // Wait for Angular to settle
            await page.WaitForSelectorAsync("app-root", new WaitForSelectorOptions { Timeout = 10_000 });
            await Task.Delay(2_500, ct);

            for (var i = 0; i < steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var step     = steps[i];
                var imgPath  = Path.Combine(workDir, $"step_{i}.png");

                if (!string.IsNullOrWhiteSpace(step.Element))
                {
                    await HighlightElementAsync(page, step.Element);
                    await Task.Delay(300, ct);
                }

                await page.ScreenshotAsync(imgPath, new ScreenshotOptions
                {
                    FullPage  = false,
                    Type      = ScreenshotType.Png,
                    OmitBackground = false,
                });

                if (!string.IsNullOrWhiteSpace(step.Element))
                    await RemoveHighlightAsync(page);

                paths.Add(imgPath);
                logger.LogDebug("VideoGen: screenshot {Index}/{Total}", i + 1, steps.Count);
            }
        }
        finally
        {
            await page.CloseAsync();
        }

        return paths;
    }

    private static Task HighlightElementAsync(IPage page, string selector) =>
        page.EvaluateExpressionAsync($$"""
            (function() {
                const el = document.querySelector('{{selector.Replace("'", "\\'")}}');
                if (!el) return;
                const r = el.getBoundingClientRect();
                el.scrollIntoView({ behavior: 'instant', block: 'center' });
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
                    'transition:none',
                ].join(';');
                document.body.appendChild(ov);
            })()
        """);

    private static Task RemoveHighlightAsync(IPage page) =>
        page.EvaluateExpressionAsync("""
            (function() {
                const el = document.getElementById('__qbe_hl__');
                if (el) el.remove();
            })()
        """);

    // ──────────────────────────────────────────────────────────
    // Step 2 — TTS audio
    // ──────────────────────────────────────────────────────────

    private async Task GenerateAudioAsync(
        List<WalkthroughStep> steps,
        string workDir,
        CancellationToken ct)
    {
        for (var i = 0; i < steps.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var step   = steps[i];
            var text   = BuildNarration(step);
            var audio  = await tts.GenerateSpeechAsync(text, ct);
            var path   = Path.Combine(workDir, $"step_{i}.mp3");
            await File.WriteAllBytesAsync(path, audio, ct);
            logger.LogDebug("VideoGen: TTS step {Index}/{Total}", i + 1, steps.Count);
        }
    }

    private static string BuildNarration(WalkthroughStep step)
    {
        var title = step.Popover.Title.TrimEnd('.');
        var desc  = step.Popover.Description.TrimEnd('.');
        return $"{title}. {desc}.";
    }

    // ──────────────────────────────────────────────────────────
    // Step 3 — ffmpeg: image + audio → per-step MP4
    // ──────────────────────────────────────────────────────────

    private async Task BuildSegmentsAsync(int count, string workDir, CancellationToken ct)
    {
        for (var i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var img  = Path.Combine(workDir, $"step_{i}.png");
            var mp3  = Path.Combine(workDir, $"step_{i}.mp3");
            var out_ = Path.Combine(workDir, $"seg_{i}.mp4");

            // scale to 1920x1080 with letterbox, ensure even dimensions
            const string vf =
                "scale=1920:1080:force_original_aspect_ratio=decrease," +
                "pad=1920:1080:(ow-iw)/2:(oh-ih)/2:black," +
                "setsar=1";

            var args = string.Join(' ',
                "-y",
                "-loop 1", $"-i \"{img}\"",
                $"-i \"{mp3}\"",
                $"-vf \"{vf}\"",
                "-c:v libx264 -preset fast -tune stillimage",
                "-c:a aac -b:a 192k",
                "-pix_fmt yuv420p",
                "-shortest",
                $"\"{out_}\"");

            await RunFfmpegAsync(args, ct);
            logger.LogDebug("VideoGen: segment {Index}/{Total}", i + 1, count);
        }
    }

    // ──────────────────────────────────────────────────────────
    // Step 4 — ffmpeg: concatenate segments
    // ──────────────────────────────────────────────────────────

    private async Task ConcatenateSegmentsAsync(
        int count, string workDir, string outputPath, CancellationToken ct)
    {
        var listPath = Path.Combine(workDir, "concat.txt");
        var lines    = Enumerable.Range(0, count)
            .Select(i => $"file '{Path.Combine(workDir, $"seg_{i}.mp4").Replace("'", "'\\''")}'");
        await File.WriteAllLinesAsync(listPath, lines, ct);

        var args = $"-y -f concat -safe 0 -i \"{listPath}\" -c copy \"{outputPath}\"";
        await RunFfmpegAsync(args, ct);
    }

    // ──────────────────────────────────────────────────────────
    // ffmpeg process helper
    // ──────────────────────────────────────────────────────────

    private async Task RunFfmpegAsync(string args, CancellationToken ct)
    {
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "ffmpeg",
                Arguments              = args,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            },
        };

        proc.Start();
        var stderr = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);

        if (proc.ExitCode != 0)
            throw new InvalidOperationException($"ffmpeg failed (exit {proc.ExitCode}): {stderr[..Math.Min(stderr.Length, 500)]}");
    }

    // ──────────────────────────────────────────────────────────
    // Browser lifecycle
    // ──────────────────────────────────────────────────────────

    private async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser is { IsConnected: true })
            return _browser;

        await _browserLock.WaitAsync();
        try
        {
            if (_browser is { IsConnected: true })
                return _browser;

            var execPath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
            LaunchOptions opts;

            if (!string.IsNullOrWhiteSpace(execPath))
            {
                opts = new LaunchOptions
                {
                    Headless        = true,
                    ExecutablePath  = execPath,
                    Args            = ["--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage"],
                };
            }
            else
            {
                logger.LogWarning("VideoGen: PUPPETEER_EXECUTABLE_PATH not set — downloading Chromium");
                var fetcher = new BrowserFetcher();
                await fetcher.DownloadAsync();
                opts = new LaunchOptions
                {
                    Headless = true,
                    Args     = ["--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage"],
                };
            }

            _browser = await Puppeteer.LaunchAsync(opts);
            logger.LogInformation("VideoGen: browser launched (pid {Pid})", _browser.Process?.Id);
            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }

    private string GetBaseUrl()
    {
        var url = configuration["FormRendererBaseUrl"];
        if (string.IsNullOrWhiteSpace(url))
            url = configuration["FrontendBaseUrl"];
        return string.IsNullOrWhiteSpace(url) ? "http://localhost:4200" : url;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser.Dispose();
        }
        _browserLock.Dispose();
    }

    // ──────────────────────────────────────────────────────────
    // Internal DTOs for JSON parsing
    // ──────────────────────────────────────────────────────────

    private record WalkthroughContent(
        [property: JsonPropertyName("appRoute")]        string? AppRoute,
        [property: JsonPropertyName("startButtonLabel")] string? StartButtonLabel,
        [property: JsonPropertyName("steps")]           List<WalkthroughStep> Steps);

    private record WalkthroughStep(
        [property: JsonPropertyName("element")]  string? Element,
        [property: JsonPropertyName("popover")]  WalkthroughPopoverDto Popover);

    private record WalkthroughPopoverDto(
        [property: JsonPropertyName("title")]       string Title,
        [property: JsonPropertyName("description")] string Description);
}
