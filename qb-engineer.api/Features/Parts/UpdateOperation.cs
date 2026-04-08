using FluentValidation;
using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record UpdateOperationCommand(int PartId, int OperationId, UpdateOperationRequestModel Data) : IRequest<OperationResponseModel>;

public class UpdateOperationValidator : AbstractValidator<UpdateOperationCommand>
{
    public UpdateOperationValidator()
    {
        RuleFor(x => x.PartId).GreaterThan(0);
        RuleFor(x => x.OperationId).GreaterThan(0);
        RuleFor(x => x.Data.StepNumber).GreaterThan(0).When(x => x.Data.StepNumber.HasValue);
        RuleFor(x => x.Data.Title).NotEmpty().MaximumLength(200).When(x => x.Data.Title is not null);
        RuleFor(x => x.Data.Instructions).MaximumLength(4000).When(x => x.Data.Instructions is not null);
        RuleFor(x => x.Data.QcCriteria).MaximumLength(1000).When(x => x.Data.QcCriteria is not null);
    }
}

public class UpdateOperationHandler(IPartRepository repo) : IRequestHandler<UpdateOperationCommand, OperationResponseModel>
{
    public async Task<OperationResponseModel> Handle(UpdateOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await repo.FindOperationAsync(request.OperationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Operation {request.OperationId} not found");

        if (operation.PartId != request.PartId)
            throw new KeyNotFoundException($"Operation {request.OperationId} does not belong to part {request.PartId}");

        var data = request.Data;

        if (data.StepNumber.HasValue) operation.StepNumber = data.StepNumber.Value;
        if (data.Title is not null) operation.Title = data.Title.Trim();
        if (data.Instructions is not null) operation.Instructions = data.Instructions.Trim();
        if (data.WorkCenterId is not null) operation.WorkCenterId = data.WorkCenterId;
        if (data.EstimatedMinutes is not null) operation.EstimatedMinutes = data.EstimatedMinutes;
        if (data.IsQcCheckpoint.HasValue) operation.IsQcCheckpoint = data.IsQcCheckpoint.Value;
        if (data.QcCriteria is not null) operation.QcCriteria = data.QcCriteria.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        var operations = await repo.GetOperationsAsync(request.PartId, cancellationToken);
        return operations.First(s => s.Id == operation.Id);
    }
}
