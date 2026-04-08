using FluentValidation;
using MediatR;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record CreateOperationCommand(int PartId, CreateOperationRequestModel Data) : IRequest<OperationResponseModel>;

public class CreateOperationValidator : AbstractValidator<CreateOperationCommand>
{
    public CreateOperationValidator()
    {
        RuleFor(x => x.PartId).GreaterThan(0);
        RuleFor(x => x.Data.StepNumber).GreaterThan(0);
        RuleFor(x => x.Data.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.Instructions).MaximumLength(4000).When(x => x.Data.Instructions is not null);
        RuleFor(x => x.Data.QcCriteria).MaximumLength(1000).When(x => x.Data.QcCriteria is not null);
    }
}

public class CreateOperationHandler(IPartRepository repo) : IRequestHandler<CreateOperationCommand, OperationResponseModel>
{
    public async Task<OperationResponseModel> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        var part = await repo.FindAsync(request.PartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.PartId} not found");

        var operation = new Operation
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

        part.Operations.Add(operation);
        await repo.SaveChangesAsync(cancellationToken);

        var operations = await repo.GetOperationsAsync(request.PartId, cancellationToken);
        return operations.First(s => s.Id == operation.Id);
    }
}
