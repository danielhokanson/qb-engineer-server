using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.RecurringOrders;

public record CreateRecurringOrderCommand(
    string Name,
    int CustomerId,
    int? ShippingAddressId,
    int IntervalDays,
    DateTime NextGenerationDate,
    string? Notes,
    List<CreateRecurringOrderLineModel> Lines) : IRequest<RecurringOrderListItemModel>;

public class CreateRecurringOrderValidator : AbstractValidator<CreateRecurringOrderCommand>
{
    public CreateRecurringOrderValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.IntervalDays).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.PartId).GreaterThan(0);
            line.RuleFor(l => l.Description).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateRecurringOrderHandler(IRecurringOrderRepository repo, ICustomerRepository customerRepo)
    : IRequestHandler<CreateRecurringOrderCommand, RecurringOrderListItemModel>
{
    public async Task<RecurringOrderListItemModel> Handle(CreateRecurringOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepo.FindAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        var ro = new RecurringOrder
        {
            Name = request.Name,
            CustomerId = request.CustomerId,
            ShippingAddressId = request.ShippingAddressId,
            IntervalDays = request.IntervalDays,
            NextGenerationDate = request.NextGenerationDate,
            Notes = request.Notes,
        };

        var lineNumber = 1;
        foreach (var line in request.Lines)
        {
            ro.Lines.Add(new RecurringOrderLine
            {
                PartId = line.PartId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineNumber = lineNumber++,
            });
        }

        await repo.AddAsync(ro, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return new RecurringOrderListItemModel(
            ro.Id, ro.Name, ro.CustomerId, customer.Name,
            ro.IntervalDays, ro.NextGenerationDate, ro.LastGeneratedDate,
            ro.IsActive, ro.Lines.Count, ro.CreatedAt);
    }
}
