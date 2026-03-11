using FluentValidation;
using MediatR;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record CreateProcessStepCommand(int PartId, CreateProcessStepRequestModel Data) : IRequest<ProcessStepResponseModel>;

public class CreateProcessStepValidator : AbstractValidator<CreateProcessStepCommand>
{
    public CreateProcessStepValidator()
    {
        RuleFor(x => x.PartId).GreaterThan(0);
        RuleFor(x => x.Data.StepNumber).GreaterThan(0);
        RuleFor(x => x.Data.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.Instructions).MaximumLength(4000).When(x => x.Data.Instructions is not null);
        RuleFor(x => x.Data.QcCriteria).MaximumLength(1000).When(x => x.Data.QcCriteria is not null);
    }
}

public class CreateProcessStepHandler(IPartRepository repo) : IRequestHandler<CreateProcessStepCommand, ProcessStepResponseModel>
{
    public async Task<ProcessStepResponseModel> Handle(CreateProcessStepCommand request, CancellationToken cancellationToken)
    {
        var part = await repo.FindAsync(request.PartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.PartId} not found");

        var step = new ProcessStep
        {
            PartId = request.PartId,
            StepNumber = request.Data.StepNumber,
            Title = request.Data.Title.Trim(),
            Instructions = request.Data.Instructions?.Trim(),
            WorkCenterId = request.Data.WorkCenterId,
            EstimatedMinutes = request.Data.EstimatedMinutes,
            IsQcCheckpoint = request.Data.IsQcCheckpoint,
            QcCriteria = request.Data.QcCriteria?.Trim(),
        };

        part.ProcessSteps.Add(step);
        await repo.SaveChangesAsync(cancellationToken);

        var steps = await repo.GetProcessStepsAsync(request.PartId, cancellationToken);
        return steps.First(s => s.Id == step.Id);
    }
}
