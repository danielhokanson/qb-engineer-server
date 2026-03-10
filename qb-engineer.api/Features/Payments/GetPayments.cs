using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Payments;

public record GetPaymentsQuery(int? CustomerId) : IRequest<List<PaymentListItemModel>>;

public class GetPaymentsHandler(IPaymentRepository repo)
    : IRequestHandler<GetPaymentsQuery, List<PaymentListItemModel>>
{
    public async Task<List<PaymentListItemModel>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.CustomerId, cancellationToken);
    }
}
