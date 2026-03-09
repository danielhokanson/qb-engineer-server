using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record UpdatePartCommand(int Id, UpdatePartRequestModel Data) : IRequest<PartDetailResponseModel>;

public class UpdatePartHandler(IPartRepository repo) : IRequestHandler<UpdatePartCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.Id} not found");

        var data = request.Data;

        if (data.Description is not null) part.Description = data.Description.Trim();
        if (data.Revision is not null) part.Revision = data.Revision.Trim();
        if (data.Status.HasValue) part.Status = data.Status.Value;
        if (data.PartType.HasValue) part.PartType = data.PartType.Value;
        if (data.Material is not null) part.Material = data.Material.Trim();
        if (data.MoldToolRef is not null) part.MoldToolRef = data.MoldToolRef.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetDetailAsync(part.Id, cancellationToken))!;
    }
}
