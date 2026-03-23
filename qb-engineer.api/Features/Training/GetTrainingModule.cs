using System.Text.Json;
using System.Text.Json.Nodes;

using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Training;

public record GetTrainingModuleQuery(int Id, int UserId, bool IsAdmin) : IRequest<TrainingModuleDetailResponseModel>;

public class GetTrainingModuleHandler(AppDbContext db)
    : IRequestHandler<GetTrainingModuleQuery, TrainingModuleDetailResponseModel>
{
    public async Task<TrainingModuleDetailResponseModel> Handle(GetTrainingModuleQuery request, CancellationToken ct)
    {
        var query = db.TrainingModules.AsNoTracking().Where(m => m.Id == request.Id);

        if (!request.IsAdmin)
            query = query.Where(m => m.IsPublished);

        var module = await query.FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Training module {request.Id} not found.");

        var progress = await db.TrainingProgress
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.ModuleId == request.Id, ct);

        var contentJson = module.ContentJson;
        if (!request.IsAdmin && module.ContentType == QBEngineer.Core.Enums.TrainingContentType.Quiz)
            contentJson = StripCorrectAnswers(contentJson);

        return new TrainingModuleDetailResponseModel(
            module.Id,
            module.Title,
            module.Slug,
            module.Summary,
            module.ContentType,
            module.CoverImageUrl,
            module.EstimatedMinutes,
            ParseJsonStringArray(module.Tags),
            module.IsPublished,
            module.IsOnboardingRequired,
            module.SortOrder,
            progress?.Status,
            progress?.QuizScore,
            progress?.CompletedAt,
            contentJson,
            ParseJsonStringArray(module.AppRoutes),
            module.CreatedAt,
            module.UpdatedAt
        );
    }

    private static string[] ParseJsonStringArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<string[]>(json) ?? []; }
        catch { return []; }
    }

    private static string StripCorrectAnswers(string contentJson)
    {
        try
        {
            var node = JsonNode.Parse(contentJson);
            if (node is not JsonObject root) return contentJson;

            var questions = root["questions"]?.AsArray();
            if (questions is null) return contentJson;

            foreach (var question in questions)
            {
                var options = question?["options"]?.AsArray();
                if (options is null) continue;
                foreach (var option in options)
                    option?.AsObject().Remove("isCorrect");
            }

            root.Remove("passingScore");
            return root.ToJsonString();
        }
        catch
        {
            return contentJson;
        }
    }
}
