using FluentValidation;
using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record UpdateBOMEntryCommand(int ParentPartId, int BomEntryId, UpdateBOMEntryRequestModel Data) : IRequest<PartDetailResponseModel>;

public class UpdateBOMEntryValidator : AbstractValidator<UpdateBOMEntryCommand>
{
    public UpdateBOMEntryValidator()
    {
        RuleFor(x => x.ParentPartId).GreaterThan(0);
        RuleFor(x => x.BomEntryId).GreaterThan(0);
        RuleFor(x => x.Data.Quantity).GreaterThan(0).When(x => x.Data.Quantity.HasValue);
        RuleFor(x => x.Data.ReferenceDesignator).MaximumLength(200).When(x => x.Data.ReferenceDesignator is not null);
        RuleFor(x => x.Data.Notes).MaximumLength(2000).When(x => x.Data.Notes is not null);
    }
}

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
        if (data.LeadTimeDays is not null) entry.LeadTimeDays = data.LeadTimeDays;
        if (data.Notes is not null) entry.Notes = data.Notes.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetDetailAsync(request.ParentPartId, cancellationToken))!;
    }
}
