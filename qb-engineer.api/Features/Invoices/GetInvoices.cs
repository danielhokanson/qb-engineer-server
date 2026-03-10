using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Invoices;

public record GetInvoicesQuery(int? CustomerId, InvoiceStatus? Status) : IRequest<List<InvoiceListItemModel>>;

public class GetInvoicesHandler(IInvoiceRepository repo)
    : IRequestHandler<GetInvoicesQuery, List<InvoiceListItemModel>>
{
    public async Task<List<InvoiceListItemModel>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.CustomerId, request.Status, cancellationToken);
    }
}
