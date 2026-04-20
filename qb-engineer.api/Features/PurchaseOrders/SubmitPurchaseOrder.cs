using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Http;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record SubmitPurchaseOrderCommand(int Id) : IRequest;

public class SubmitPurchaseOrderHandler(
    IPurchaseOrderRepository repo,
    IApprovalService approvalService,
    IHttpContextAccessor httpContext)
    : IRequestHandler<SubmitPurchaseOrderCommand>
{
    public async Task Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repo.FindWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found");

        if (po.Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft purchase orders can be submitted");

        po.Status = PurchaseOrderStatus.Submitted;
        po.SubmittedDate = DateTimeOffset.UtcNow;

        await repo.SaveChangesAsync(cancellationToken);

        // Submit for approval if a workflow matches. Approval is advisory here —
        // it does not gate the PO submission itself. This populates the generic
        // /approvals inbox so Managers can review high-value POs.
        var claim = httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out var userId) && userId > 0)
        {
            var totalAmount = po.Lines.Sum(l => l.OrderedQuantity * l.UnitPrice);
            var summary = $"PO {po.PONumber} — {po.Lines.Count} line(s)";
            await approvalService.SubmitForApprovalAsync(
                "PurchaseOrder", po.Id, userId, totalAmount, summary, cancellationToken);
        }
    }
}
