using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Invoices;

public record VoidInvoiceCommand(int Id) : IRequest;

public class VoidInvoiceHandler(IInvoiceRepository repo)
    : IRequestHandler<VoidInvoiceCommand>
{
    public async Task Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await repo.FindWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {request.Id} not found");

        if (invoice.PaymentApplications.Any())
            throw new InvalidOperationException("Cannot void an invoice with payments applied");

        invoice.Status = InvoiceStatus.Voided;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
