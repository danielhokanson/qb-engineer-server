using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Jobs;

public class OverdueInvoiceJob(
    AppDbContext db,
    ILogger<OverdueInvoiceJob> logger)
{
    public async Task MarkOverdueInvoicesAsync()
    {
        var now = DateTime.UtcNow;
        var overdueInvoices = await db.Invoices
            .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < now)
            .ToListAsync();

        if (overdueInvoices.Count == 0)
        {
            logger.LogInformation("No invoices to mark as overdue");
            return;
        }

        foreach (var invoice in overdueInvoices)
        {
            invoice.Status = InvoiceStatus.Overdue;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Marked {Count} invoices as overdue", overdueInvoices.Count);
    }
}
