using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Ai;

public record BulkIndexDocumentsCommand(string? EntityType = null) : IRequest<int>;

public class BulkIndexDocumentsValidator : AbstractValidator<BulkIndexDocumentsCommand>
{
    public BulkIndexDocumentsValidator()
    {
        RuleFor(x => x.EntityType).MaximumLength(50);
    }
}

public class BulkIndexDocumentsHandler(
    AppDbContext db,
    IMediator mediator,
    ILogger<BulkIndexDocumentsHandler> logger) : IRequestHandler<BulkIndexDocumentsCommand, int>
{
    private static readonly string[] SupportedEntityTypes = ["Job", "Part", "FileAttachment", "Customer", "Asset", "Documentation"];

    public async Task<int> Handle(BulkIndexDocumentsCommand request, CancellationToken ct)
    {
        var entityTypes = request.EntityType is not null
            ? [request.EntityType]
            : SupportedEntityTypes;

        var totalIndexed = 0;

        foreach (var entityType in entityTypes)
        {
            // Documentation uses its own handler (reads from filesystem, not DB)
            if (entityType == "Documentation")
            {
                try
                {
                    var chunkCount = await mediator.Send(new IndexDocumentationCommand(), ct);
                    totalIndexed += chunkCount;
                    logger.LogInformation("Bulk indexed documentation: {ChunkCount} chunks", chunkCount);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to bulk index documentation");
                }
                continue;
            }

            var entityIds = await GetEntityIdsAsync(entityType, ct);
            logger.LogInformation("Bulk indexing {Count} {EntityType} entities", entityIds.Count, entityType);

            foreach (var entityId in entityIds)
            {
                try
                {
                    var chunkCount = await mediator.Send(
                        new IndexDocumentCommand(entityType, entityId), ct);
                    totalIndexed += chunkCount;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to index {EntityType} #{EntityId}", entityType, entityId);
                }
            }
        }

        logger.LogInformation("Bulk indexing complete: {TotalChunks} total chunks", totalIndexed);
        return totalIndexed;
    }

    private async Task<List<int>> GetEntityIdsAsync(string entityType, CancellationToken ct)
    {
        return entityType switch
        {
            "Job" => await db.Jobs.Select(j => j.Id).ToListAsync(ct),
            "Part" => await db.Parts.Select(p => p.Id).ToListAsync(ct),
            "FileAttachment" => await db.FileAttachments.Select(f => f.Id).ToListAsync(ct),
            "Customer" => await db.Customers.Select(c => c.Id).ToListAsync(ct),
            "Asset" => await db.Assets.Select(a => a.Id).ToListAsync(ct),
            _ => [],
        };
    }
}
