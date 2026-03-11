using System.Text;

using FluentValidation;
using MediatR;
using Pgvector;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Ai;

public record RagSearchCommand(
    string Query,
    string? EntityTypeFilter = null,
    bool IncludeAnswer = false) : IRequest<RagSearchResponseModel>;

public class RagSearchValidator : AbstractValidator<RagSearchCommand>
{
    public RagSearchValidator()
    {
        RuleFor(x => x.Query).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EntityTypeFilter).MaximumLength(50);
    }
}

public class RagSearchHandler(
    IAiService aiService,
    IEmbeddingRepository embeddingRepo) : IRequestHandler<RagSearchCommand, RagSearchResponseModel>
{
    public async Task<RagSearchResponseModel> Handle(RagSearchCommand request, CancellationToken ct)
    {
        var queryEmbeddingArray = await aiService.GetEmbeddingAsync(request.Query, ct);
        var queryVector = new Vector(queryEmbeddingArray);

        var similar = await embeddingRepo.SearchSimilarAsync(
            queryVector, 10, request.EntityTypeFilter, ct);

        var results = similar.Select((doc, index) =>
        {
            // Cosine distance ranges 0-2; convert to similarity score 0-1
            // Since pgvector orders by distance ascending, approximate score from position
            var score = 1.0 - (index * 0.05);
            return new RagSearchResultModel(
                doc.EntityType,
                doc.EntityId,
                doc.ChunkText,
                doc.SourceField,
                Math.Max(0, score));
        }).ToList();

        string? generatedAnswer = null;

        if (request.IncludeAnswer && results.Count > 0)
        {
            var contextChunks = results.Take(5);
            var contextBuilder = new StringBuilder();
            foreach (var chunk in contextChunks)
            {
                contextBuilder.AppendLine($"[{chunk.EntityType} #{chunk.EntityId} — {chunk.SourceField}]:");
                contextBuilder.AppendLine(chunk.ChunkText);
                contextBuilder.AppendLine();
            }

            var prompt = $"""
                You are a helpful assistant for a manufacturing operations platform called QB Engineer.
                Use the following context to answer the user's question. If the context doesn't contain
                relevant information, say so. Be concise and factual.

                Context:
                {contextBuilder}

                Question: {request.Query}

                Answer:
                """;

            generatedAnswer = await aiService.GenerateTextAsync(prompt, ct);
        }

        return new RagSearchResponseModel(results, generatedAnswer);
    }
}
