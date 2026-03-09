using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record DeleteBOMEntryCommand(int ParentPartId, int BomEntryId) : IRequest<PartDetailResponseModel>;

public class DeleteBOMEntryHandler(IPartRepository repo) : IRequestHandler<DeleteBOMEntryCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(DeleteBOMEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repo.FindBomEntryAsync(request.BomEntryId, request.ParentPartId, cancellationToken)
            ?? throw new KeyNotFoundException($"BOM entry {request.BomEntryId} not found on part {request.ParentPartId}");

        await repo.RemoveBomEntryAsync(entry);

        return (await repo.GetDetailAsync(request.ParentPartId, cancellationToken))!;
    }
}
