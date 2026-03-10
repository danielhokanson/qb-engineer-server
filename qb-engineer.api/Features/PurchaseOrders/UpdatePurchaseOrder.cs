using FluentValidation;
using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record UpdatePurchaseOrderCommand(int Id, string? Notes, DateTime? ExpectedDeliveryDate) : IRequest;

public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}

public class UpdatePurchaseOrderHandler(IPurchaseOrderRepository repo)
    : IRequestHandler<UpdatePurchaseOrderCommand>
{
    public async Task Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found");

        if (po.Status != PurchaseOrderStatus.Draft && po.Status != PurchaseOrderStatus.Submitted)
            throw new InvalidOperationException("Can only update Draft or Submitted purchase orders");

        if (request.Notes != null) po.Notes = request.Notes;
        if (request.ExpectedDeliveryDate.HasValue) po.ExpectedDeliveryDate = request.ExpectedDeliveryDate;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
