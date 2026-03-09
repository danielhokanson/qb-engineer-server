using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record UpdateBOMEntryCommand(int ParentPartId, int BomEntryId, UpdateBOMEntryRequestModel Data) : IRequest<PartDetailResponseModel>;

public class UpdateBOMEntryHandler(IPartRepository repo) : IRequestHandler<UpdateBOMEntryCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(UpdateBOMEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repo.FindBomEntryAsync(request.BomEntryId, request.ParentPartId, cancellationToken)
            ?? throw new KeyNotFoundException($"BOM entry {request.BomEntryId} not found on part {request.ParentPartId}");

        var data = request.Data;

        if (data.Quantity.HasValue) entry.Quantity = data.Quantity.Value;
        if (data.ReferenceDesignator is not null) entry.ReferenceDesignator = data.ReferenceDesignator.Trim();
        if (data.SourceType.HasValue) entry.SourceType = data.SourceType.Value;
        if (data.Notes is not null) entry.Notes = data.Notes.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetDetailAsync(request.ParentPartId, cancellationToken))!;
    }
}
