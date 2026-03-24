using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Training;

// ── Query / Response ─────────────────────────────────────────────────────────

public record GetVideoGenerationStatusQuery(int ModuleId) : IRequest<VideoStatusResponseModel>;

public record VideoStatusResponseModel(
    int ModuleId,
    string Status,
    string? PresignedUrl,
    string? ErrorMessage);

// ── Handler ──────────────────────────────────────────────────────────────────

public class GetVideoGenerationStatusHandler(
    AppDbContext db,
    IStorageService storage) : IRequestHandler<GetVideoGenerationStatusQuery, VideoStatusResponseModel>
{
    private const string Bucket = "qb-engineer-training-videos";

    public async Task<VideoStatusResponseModel> Handle(
        GetVideoGenerationStatusQuery request,
        CancellationToken ct)
    {
        var module = await db.TrainingModules
            .AsNoTracking()
            .Where(m => m.Id == request.ModuleId)
            .Select(m => new { m.VideoGenerationStatus, m.VideoMinioKey, m.VideoGenerationError })
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Training module {request.ModuleId} not found.");

        string? presignedUrl = null;
        if (module.VideoGenerationStatus == VideoGenerationStatus.Done
            && module.VideoMinioKey is not null)
        {
            presignedUrl = await storage.GetPresignedUrlAsync(Bucket, module.VideoMinioKey, 3600, ct);
        }

        return new VideoStatusResponseModel(
            request.ModuleId,
            module.VideoGenerationStatus.ToString(),
            presignedUrl,
            module.VideoGenerationError);
    }
}
