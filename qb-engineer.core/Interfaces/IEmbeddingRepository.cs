using Pgvector;

using QBEngineer.Core.Entities;

namespace QBEngineer.Core.Interfaces;

public interface IEmbeddingRepository
{
    Task<List<DocumentEmbedding>> SearchSimilarAsync(Vector queryVector, int topK, string? entityTypeFilter, CancellationToken ct);
    Task UpsertEmbeddingsAsync(string entityType, int entityId, List<DocumentEmbedding> embeddings, CancellationToken ct);
    Task DeleteEmbeddingsAsync(string entityType, int entityId, CancellationToken ct);
    Task<int> GetEmbeddingCountAsync(CancellationToken ct);
}
