using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record CreatePurchaseOrderCommand(
    int VendorId,
    int? JobId,
    string? Notes,
    List<CreatePurchaseOrderLineModel> Lines) : IRequest<PurchaseOrderListItemModel>;

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.PartId).GreaterThan(0);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreatePurchaseOrderHandler(IPurchaseOrderRepository poRepo, IVendorRepository vendorRepo, IPartRepository partRepo)
    : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderListItemModel>
{
    public async Task<PurchaseOrderListItemModel> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var vendor = await vendorRepo.FindAsync(request.VendorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor {request.VendorId} not found");

        var poNumber = await poRepo.GenerateNextPONumberAsync(cancellationToken);

        var po = new PurchaseOrder
        {
            PONumber = poNumber,
            VendorId = request.VendorId,
            JobId = request.JobId,
            Notes = request.Notes,
        };

        foreach (var line in request.Lines)
        {
            var part = await partRepo.FindAsync(line.PartId, cancellationToken)
                ?? throw new KeyNotFoundException($"Part {line.PartId} not found");

            po.Lines.Add(new PurchaseOrderLine
            {
                PartId = line.PartId,
                Description = line.Description ?? part.Description,
                OrderedQuantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Notes = line.Notes,
            });
        }

        await poRepo.AddAsync(po, cancellationToken);
        await poRepo.SaveChangesAsync(cancellationToken);

        return new PurchaseOrderListItemModel(
            po.Id, po.PONumber, po.VendorId, vendor.CompanyName,
            po.JobId, null, po.Status.ToString(),
            po.Lines.Count,
            po.Lines.Sum(l => l.OrderedQuantity),
            0, null, po.CreatedAt);
    }
}
