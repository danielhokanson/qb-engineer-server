using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record CreatePartCommand(
    string PartNumber,
    string Description,
    string? Revision,
    PartType PartType,
    string? Material,
    string? MoldToolRef) : IRequest<PartDetailResponseModel>;

public class CreatePartCommandValidator : AbstractValidator<CreatePartCommand>
{
    public CreatePartCommandValidator()
    {
        RuleFor(x => x.PartNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Revision).MaximumLength(10).When(x => x.Revision is not null);
        RuleFor(x => x.Material).MaximumLength(200).When(x => x.Material is not null);
    }
}

public class CreatePartHandler(IPartRepository repo) : IRequestHandler<CreatePartCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        if (await repo.PartNumberExistsAsync(request.PartNumber, null, cancellationToken))
            throw new InvalidOperationException($"Part number '{request.PartNumber}' already exists");

        var part = new Part
        {
            PartNumber = request.PartNumber.Trim().ToUpper(),
            Description = request.Description.Trim(),
            Revision = request.Revision?.Trim() ?? "A",
            PartType = request.PartType,
            Status = PartStatus.Draft,
            Material = request.Material?.Trim(),
            MoldToolRef = request.MoldToolRef?.Trim(),
        };

        await repo.AddAsync(part, cancellationToken);

        return (await repo.GetDetailAsync(part.Id, cancellationToken))!;
    }
}
