using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Features.Ai;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Jobs;

public class DocumentIndexJob(
    AppDbContext db,
    IMediator mediator,
    IAiService aiService,
    ILogger<DocumentIndexJob> logger)
{
    private static readonly string[] SupportedEntityTypes = ["Job", "Part", "Customer", "Asset"];

    public async Task IndexDocumentationAsync()
    {
        var available = await aiService.IsAvailableAsync(CancellationToken.None);
        if (!available)
        {
            logger.LogInformation("AI service unavailable — skipping documentation indexing");
            return;
        }

        try
        {
            var chunksIndexed = await mediator.Send(new IndexDocumentationCommand(), CancellationToken.None);
            if (chunksIndexed > 0)
                logger.LogInformation("Documentation indexing complete: {ChunkCount} chunks", chunksIndexed);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Documentation indexing failed");
        }
    }

    public async Task IndexRecentlyUpdatedAsync()
    {
        var available = await aiService.IsAvailableAsync(CancellationToken.None);
        if (!available)
        {
            logger.LogInformation("AI service unavailable — skipping document indexing");
            return;
        }

        var since = DateTimeOffset.UtcNow.AddMinutes(-35); // Overlap slightly to avoid gaps
        var totalIndexed = 0;

        foreach (var entityType in SupportedEntityTypes)
        {
            try
            {
                var updatedIds = await GetRecentlyUpdatedIdsAsync(entityType, since);

                if (updatedIds.Count == 0)
                    continue;

                logger.LogInformation(
                    "Document indexing: {Count} {EntityType} entities updated since {Since:u}",
                    updatedIds.Count, entityType, since);

                foreach (var entityId in updatedIds)
                {
                    try
                    {
                        var chunkCount = await mediator.Send(
                            new IndexDocumentCommand(entityType, entityId),
                            CancellationToken.None);
                        totalIndexed += chunkCount;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to index {EntityType} #{EntityId}", entityType, entityId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to query recently updated {EntityType} entities", entityType);
            }
        }

        if (totalIndexed > 0)
        {
            logger.LogInformation("Document indexing complete: {TotalChunks} chunks indexed", totalIndexed);
        }
    }

    private async Task<List<int>> GetRecentlyUpdatedIdsAsync(string entityType, DateTimeOffset since)
    {
        return entityType switch
        {
            "Job" => await db.Jobs
                .Where(j => j.UpdatedAt >= since)
                .Select(j => j.Id)
                .ToListAsync(),
            "Part" => await db.Parts
                .Where(p => p.UpdatedAt >= since)
                .Select(p => p.Id)
                .ToListAsync(),
            "Customer" => await db.Customers
                .Where(c => c.UpdatedAt >= since)
                .Select(c => c.Id)
                .ToListAsync(),
            "Asset" => await db.Assets
                .Where(a => a.UpdatedAt >= since)
                .Select(a => a.Id)
                .ToListAsync(),
            _ => [],
        };
    }
}
