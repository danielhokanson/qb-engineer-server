using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record UpdateReferenceDataCommand(
    int Id,
    string? Label,
    int? SortOrder,
    bool? IsActive,
    string? Metadata) : IRequest<ReferenceDataResponseModel>;

public class UpdateReferenceDataValidator : AbstractValidator<UpdateReferenceDataCommand>
{
    public UpdateReferenceDataValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Label).MaximumLength(200).When(x => x.Label is not null);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).When(x => x.SortOrder.HasValue);
    }
}

public class UpdateReferenceDataHandler(AppDbContext db)
    : IRequestHandler<UpdateReferenceDataCommand, ReferenceDataResponseModel>
{
    public async Task<ReferenceDataResponseModel> Handle(UpdateReferenceDataCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.ReferenceData.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Reference data entry with ID {request.Id} not found.");

        if (request.Label is not null) entry.Label = request.Label;
        if (request.SortOrder.HasValue) entry.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) entry.IsActive = request.IsActive.Value;
        if (request.Metadata is not null) entry.Metadata = request.Metadata;

        await db.SaveChangesAsync(cancellationToken);

        return new ReferenceDataResponseModel(
            entry.Id, entry.Code, entry.Label, entry.SortOrder, entry.IsActive, entry.IsSeedData, entry.Metadata);
    }
}
