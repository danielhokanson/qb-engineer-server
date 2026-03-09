using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record CreateBOMEntryCommand(int ParentPartId, CreateBOMEntryRequestModel Data) : IRequest<PartDetailResponseModel>;

public class CreateBOMEntryHandler(IPartRepository repo) : IRequestHandler<CreateBOMEntryCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(CreateBOMEntryCommand request, CancellationToken cancellationToken)
    {
        var parent = await repo.FindAsync(request.ParentPartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.ParentPartId} not found");

        if (request.Data.ChildPartId == request.ParentPartId)
            throw new InvalidOperationException("A part cannot reference itself in its BOM");

        var child = await repo.FindAsync(request.Data.ChildPartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Child part {request.Data.ChildPartId} not found");

        var maxSort = await repo.GetMaxBomSortOrderAsync(request.ParentPartId, cancellationToken);

        var entry = new BOMEntry
        {
            ParentPartId = request.ParentPartId,
            ChildPartId = request.Data.ChildPartId,
            Quantity = request.Data.Quantity,
            ReferenceDesignator = request.Data.ReferenceDesignator?.Trim(),
            SortOrder = maxSort + 1,
            SourceType = request.Data.SourceType,
            Notes = request.Data.Notes?.Trim(),
        };

        await repo.AddBomEntryAsync(entry, cancellationToken);

        return (await repo.GetDetailAsync(request.ParentPartId, cancellationToken))!;
    }
}
