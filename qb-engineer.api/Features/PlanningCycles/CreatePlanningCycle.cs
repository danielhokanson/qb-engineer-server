using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PlanningCycles;

public record CreatePlanningCycleCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Goals,
    int? DurationDays) : IRequest<PlanningCycleListItemModel>;

public class CreatePlanningCycleValidator : AbstractValidator<CreatePlanningCycleCommand>
{
    public CreatePlanningCycleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");
        RuleFor(x => x.Goals).MaximumLength(2000);
    }
}

public class CreatePlanningCycleHandler(IPlanningCycleRepository repo)
    : IRequestHandler<CreatePlanningCycleCommand, PlanningCycleListItemModel>
{
    public async Task<PlanningCycleListItemModel> Handle(CreatePlanningCycleCommand request, CancellationToken cancellationToken)
    {
        var cycle = new PlanningCycle
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Goals = request.Goals,
            Status = PlanningCycleStatus.Planning,
            DurationDays = request.DurationDays ?? (int)(request.EndDate - request.StartDate).TotalDays,
        };

        await repo.AddAsync(cycle, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return new PlanningCycleListItemModel(
            cycle.Id, cycle.Name, cycle.StartDate, cycle.EndDate,
            cycle.Status.ToString(), 0, 0, cycle.CreatedAt);
    }
}
