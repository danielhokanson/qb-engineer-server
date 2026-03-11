using FluentValidation;
using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record UpdateProcessStepCommand(int PartId, int StepId, UpdateProcessStepRequestModel Data) : IRequest<ProcessStepResponseModel>;

public class UpdateProcessStepValidator : AbstractValidator<UpdateProcessStepCommand>
{
    public UpdateProcessStepValidator()
    {
        RuleFor(x => x.PartId).GreaterThan(0);
        RuleFor(x => x.StepId).GreaterThan(0);
        RuleFor(x => x.Data.StepNumber).GreaterThan(0).When(x => x.Data.StepNumber.HasValue);
        RuleFor(x => x.Data.Title).NotEmpty().MaximumLength(200).When(x => x.Data.Title is not null);
        RuleFor(x => x.Data.Instructions).MaximumLength(4000).When(x => x.Data.Instructions is not null);
        RuleFor(x => x.Data.QcCriteria).MaximumLength(1000).When(x => x.Data.QcCriteria is not null);
    }
}

public class UpdateProcessStepHandler(IPartRepository repo) : IRequestHandler<UpdateProcessStepCommand, ProcessStepResponseModel>
{
    public async Task<ProcessStepResponseModel> Handle(UpdateProcessStepCommand request, CancellationToken cancellationToken)
    {
        var step = await repo.FindProcessStepAsync(request.StepId, cancellationToken)
            ?? throw new KeyNotFoundException($"Process step {request.StepId} not found");

        if (step.PartId != request.PartId)
            throw new KeyNotFoundException($"Process step {request.StepId} does not belong to part {request.PartId}");

        var data = request.Data;

        if (data.StepNumber.HasValue) step.StepNumber = data.StepNumber.Value;
        if (data.Title is not null) step.Title = data.Title.Trim();
        if (data.Instructions is not null) step.Instructions = data.Instructions.Trim();
        if (data.WorkCenterId is not null) step.WorkCenterId = data.WorkCenterId;
        if (data.EstimatedMinutes is not null) step.EstimatedMinutes = data.EstimatedMinutes;
        if (data.IsQcCheckpoint.HasValue) step.IsQcCheckpoint = data.IsQcCheckpoint.Value;
        if (data.QcCriteria is not null) step.QcCriteria = data.QcCriteria.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        var steps = await repo.GetProcessStepsAsync(request.PartId, cancellationToken);
        return steps.First(s => s.Id == step.Id);
    }
}
