using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Invoices;

public record CreateInvoiceCommand(
    int CustomerId,
    int? SalesOrderId,
    int? ShipmentId,
    DateTimeOffset InvoiceDate,
    DateTimeOffset DueDate,
    string? CreditTerms,
    decimal TaxRate,
    string? Notes,
    List<CreateInvoiceLineModel> Lines) : IRequest<InvoiceListItemModel>;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).LessThan(1);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate);
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateInvoiceHandler(IInvoiceRepository repo, ICustomerRepository customerRepo)
    : IRequestHandler<CreateInvoiceCommand, InvoiceListItemModel>
{
    public async Task<InvoiceListItemModel> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepo.FindAsync(request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        var invoiceNumber = await repo.GenerateNextInvoiceNumberAsync(cancellationToken);

        CreditTerms? creditTerms = request.CreditTerms != null
            ? Enum.Parse<CreditTerms>(request.CreditTerms, true)
            : null;

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = request.CustomerId,
            SalesOrderId = request.SalesOrderId,
            ShipmentId = request.ShipmentId,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            CreditTerms = creditTerms,
            TaxRate = request.TaxRate,
            Notes = request.Notes,
        };

        var lineNumber = 1;
        foreach (var line in request.Lines)
        {
            invoice.Lines.Add(new InvoiceLine
            {
                PartId = line.PartId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                LineNumber = lineNumber++,
            });
        }

        await repo.AddAsync(invoice, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        var total = invoice.Lines.Sum(l => l.Quantity * l.UnitPrice) * (1 + invoice.TaxRate);

        return new InvoiceListItemModel(
            invoice.Id, invoice.InvoiceNumber, invoice.CustomerId, customer.Name,
            invoice.Status.ToString(), invoice.InvoiceDate, invoice.DueDate,
            total, 0, total, invoice.CreatedAt);
    }
}
