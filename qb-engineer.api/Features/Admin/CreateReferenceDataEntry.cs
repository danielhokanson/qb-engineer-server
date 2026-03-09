using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record CreateReferenceDataCommand(
    string GroupCode,
    string Code,
    string Label,
    int SortOrder,
    string? Metadata) : IRequest<ReferenceDataResponseModel>;

public class CreateReferenceDataHandler(AppDbContext db)
    : IRequestHandler<CreateReferenceDataCommand, ReferenceDataResponseModel>
{
    public async Task<ReferenceDataResponseModel> Handle(CreateReferenceDataCommand request, CancellationToken cancellationToken)
    {
        var exists = await db.ReferenceData
            .AnyAsync(r => r.GroupCode == request.GroupCode && r.Code == request.Code, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Reference data entry '{request.Code}' already exists in group '{request.GroupCode}'.");

        var entry = new QBEngineer.Core.Entities.ReferenceData
        {
            GroupCode = request.GroupCode,
            Code = request.Code,
            Label = request.Label,
            SortOrder = request.SortOrder,
            Metadata = request.Metadata,
        };

        db.ReferenceData.Add(entry);
        await db.SaveChangesAsync(cancellationToken);

        return new ReferenceDataResponseModel(
            entry.Id, entry.Code, entry.Label, entry.SortOrder, entry.IsActive, entry.Metadata);
    }
}
