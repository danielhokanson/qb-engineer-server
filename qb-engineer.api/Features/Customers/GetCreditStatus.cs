using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record GetCreditStatusQuery(int CustomerId) : IRequest<CreditStatusResponseModel>;

public class GetCreditStatusHandler(AppDbContext db) : IRequestHandler<GetCreditStatusQuery, CreditStatusResponseModel>
{
    public async Task<CreditStatusResponseModel> Handle(GetCreditStatusQuery request, CancellationToken ct)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        // Sum outstanding invoice balances: line totals * (1 + tax rate) minus payments applied
        var openInvoices = await db.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == request.CustomerId
                && i.Status != InvoiceStatus.Paid
                && i.Status != InvoiceStatus.Voided)
            .Select(i => new
            {
                LineTotal = i.Lines.Sum(l => l.Quantity * l.UnitPrice),
                TaxRate = i.TaxRate,
                Paid = i.PaymentApplications.Sum(pa => pa.Amount),
            })
            .ToListAsync(ct);

        var openArBalance = openInvoices.Sum(i => i.LineTotal * (1 + i.TaxRate) - i.Paid);

        // Sum pending sales order totals
        var pendingOrders = await db.SalesOrders
            .AsNoTracking()
            .Where(so => so.CustomerId == request.CustomerId
                && so.Status != SalesOrderStatus.Cancelled
                && so.Status != SalesOrderStatus.Completed)
            .Select(so => new
            {
                LineTotal = so.Lines.Sum(l => l.Quantity * l.UnitPrice),
                TaxRate = so.TaxRate,
            })
            .ToListAsync(ct);

        var pendingOrdersTotal = pendingOrders.Sum(so => so.LineTotal * (1 + so.TaxRate));

        var totalExposure = openArBalance + pendingOrdersTotal;
        var availableCredit = (customer.CreditLimit ?? 0) - totalExposure;
        var utilizationPercent = customer.CreditLimit > 0
            ? totalExposure / customer.CreditLimit.Value * 100
            : 0;
        var isOverLimit = customer.CreditLimit.HasValue && totalExposure > customer.CreditLimit.Value;

        var riskLevel = customer.IsOnCreditHold ? CreditRisk.OnHold
            : isOverLimit ? CreditRisk.High
            : utilizationPercent > 80 ? CreditRisk.Medium
            : CreditRisk.Low;

        return new CreditStatusResponseModel
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CreditLimit = customer.CreditLimit,
            OpenArBalance = openArBalance,
            PendingOrdersTotal = pendingOrdersTotal,
            TotalExposure = totalExposure,
            AvailableCredit = availableCredit,
            UtilizationPercent = utilizationPercent,
            IsOnHold = customer.IsOnCreditHold,
            HoldReason = customer.CreditHoldReason,
            IsOverLimit = isOverLimit,
            RiskLevel = riskLevel,
        };
    }
}
