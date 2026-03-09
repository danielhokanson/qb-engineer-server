using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TrackTypes;

public record UpdateTrackTypeCommand(
    int Id,
    string Name,
    string Code,
    string? Description,
    List<StageRequestModel> Stages) : IRequest<TrackTypeResponseModel>;

public class UpdateTrackTypeHandler(
    ITrackTypeRepository repo) : IRequestHandler<UpdateTrackTypeCommand, TrackTypeResponseModel>
{
    public async Task<TrackTypeResponseModel> Handle(UpdateTrackTypeCommand request, CancellationToken ct)
    {
        var trackType = await repo.FindAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Track type with ID {request.Id} not found.");

        if (await repo.CodeExistsAsync(request.Code, request.Id, ct))
            throw new InvalidOperationException($"Track type with code '{request.Code}' already exists.");

        trackType.Name = request.Name;
        trackType.Code = request.Code;
        trackType.Description = request.Description;

        // Mark removed stages as inactive
        var requestedCodes = request.Stages.Select(s => s.Code).ToHashSet();
        foreach (var existing in trackType.Stages)
        {
            if (!requestedCodes.Contains(existing.Code))
                existing.IsActive = false;
        }

        // Update existing stages and add new ones
        foreach (var stageReq in request.Stages)
        {
            var existing = trackType.Stages.FirstOrDefault(s => s.Code == stageReq.Code);
            if (existing != null)
            {
                existing.Name = stageReq.Name;
                existing.SortOrder = stageReq.SortOrder;
                existing.Color = stageReq.Color;
                existing.WIPLimit = stageReq.WIPLimit;
                existing.IsIrreversible = stageReq.IsIrreversible;
                existing.IsActive = true;
            }
            else
            {
                trackType.Stages.Add(new JobStage
                {
                    Name = stageReq.Name,
                    Code = stageReq.Code,
                    SortOrder = stageReq.SortOrder,
                    Color = stageReq.Color,
                    WIPLimit = stageReq.WIPLimit,
                    IsIrreversible = stageReq.IsIrreversible,
                });
            }
        }

        await repo.SaveChangesAsync(ct);
        return (await repo.GetByIdAsync(trackType.Id, ct))!;
    }
}
