using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.SalesOrders;

public record GetPendingBackToBacksQuery : IRequest<IReadOnlyList<BackToBackStatusResponseModel>>;

public class GetPendingBackToBacksHandler(IBackToBackService backToBackService) : IRequestHandler<GetPendingBackToBacksQuery, IReadOnlyList<BackToBackStatusResponseModel>>
{
    public async Task<IReadOnlyList<BackToBackStatusResponseModel>> Handle(GetPendingBackToBacksQuery query, CancellationToken cancellationToken)
    {
        return await backToBackService.GetPendingBackToBacksAsync(cancellationToken);
    }
}
