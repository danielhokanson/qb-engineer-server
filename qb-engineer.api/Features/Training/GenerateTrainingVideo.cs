using Hangfire;

using MediatR;

using Microsoft.AspNetCore.Identity;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Training;

// ── Command ──────────────────────────────────────────────────────────────────

public record GenerateTrainingVideoCommand(int ModuleId, string JwtToken) : IRequest;

// ── Handler ──────────────────────────────────────────────────────────────────

public class GenerateTrainingVideoHandler(
    AppDbContext db,
    IBackgroundJobClient jobs) : IRequestHandler<GenerateTrainingVideoCommand>
{
    public async Task Handle(GenerateTrainingVideoCommand request, CancellationToken ct)
    {
        var module = await db.TrainingModules.FindAsync([request.ModuleId], ct)
            ?? throw new KeyNotFoundException($"Training module {request.ModuleId} not found.");

        if (module.ContentType != TrainingContentType.Walkthrough && module.ContentType != TrainingContentType.Video)
            throw new InvalidOperationException("Video generation is only available for Walkthrough and Video modules.");

        module.VideoGenerationStatus = VideoGenerationStatus.Pending;
        module.VideoGenerationError  = null;
        await db.SaveChangesAsync(ct);

        jobs.Enqueue<TrainingVideoGenerationJob>(
            j => j.ExecuteAsync(request.ModuleId, request.JwtToken, CancellationToken.None));
    }
}

// ── Hangfire Job ─────────────────────────────────────────────────────────────

public class TrainingVideoGenerationJob(
    AppDbContext db,
    ITrainingVideoGeneratorService videoGenerator,
    IStorageService storage,
    ILogger<TrainingVideoGenerationJob> logger)
{
    private const string Bucket = "qb-engineer-training-videos";

    [Queue("video")]
    public async Task ExecuteAsync(int moduleId, string jwtToken, CancellationToken ct)
    {
        var module = await db.TrainingModules.FindAsync([moduleId], ct);
        if (module is null)
        {
            logger.LogWarning("VideoJob: module {Id} not found", moduleId);
            return;
        }

        module.VideoGenerationStatus = VideoGenerationStatus.Processing;
        await db.SaveChangesAsync(ct);

        try
        {
            await storage.EnsureBucketExistsAsync(Bucket, ct);

            var mp4Bytes = await videoGenerator.GenerateVideoAsync(module, jwtToken, ct);

            var objectKey = $"module-{moduleId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.mp4";
            await storage.UploadAsync(Bucket, objectKey, new MemoryStream(mp4Bytes), "video/mp4", ct);

            module.VideoMinioKey        = objectKey;
            module.VideoGenerationStatus = VideoGenerationStatus.Done;
            module.VideoGenerationError  = null;

            logger.LogInformation("VideoJob: module {Id} done — {Key}", moduleId, objectKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VideoJob: module {Id} failed", moduleId);
            module.VideoGenerationStatus = VideoGenerationStatus.Failed;
            module.VideoGenerationError  = ex.Message[..Math.Min(ex.Message.Length, 500)];
        }

        await db.SaveChangesAsync(ct);
    }
}
