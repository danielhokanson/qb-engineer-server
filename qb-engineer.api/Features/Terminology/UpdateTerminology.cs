using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Terminology;

public record UpdateTerminologyCommand(List<TerminologyEntryRequestModel> Entries)
    : IRequest<List<TerminologyEntryResponseModel>>;

public class UpdateTerminologyValidator : AbstractValidator<UpdateTerminologyCommand>
{
    public UpdateTerminologyValidator()
    {
        RuleFor(x => x.Entries).NotEmpty().WithMessage("At least one terminology entry is required");
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.Key).NotEmpty().MaximumLength(100);
            entry.RuleFor(e => e.Label).NotEmpty().MaximumLength(200);
        });
    }
}

public class UpdateTerminologyHandler(ITerminologyRepository repo)
    : IRequestHandler<UpdateTerminologyCommand, List<TerminologyEntryResponseModel>>
{
    public async Task<List<TerminologyEntryResponseModel>> Handle(
        UpdateTerminologyCommand request, CancellationToken cancellationToken)
    {
        foreach (var entry in request.Entries)
        {
            var existing = await repo.FindByKeyAsync(entry.Key, cancellationToken);
            if (existing is not null)
            {
                existing.Label = entry.Label;
            }
            else
            {
                await repo.AddAsync(new TerminologyEntry
                {
                    Key = entry.Key,
                    Label = entry.Label,
                }, cancellationToken);
            }
        }

        await repo.SaveChangesAsync(cancellationToken);

        return await repo.GetAllAsync(cancellationToken);
    }
}
