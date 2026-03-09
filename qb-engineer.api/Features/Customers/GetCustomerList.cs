using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Customers;

public record GetCustomerListQuery(string? Search, bool? IsActive) : IRequest<List<CustomerListItemModel>>;

public class GetCustomerListHandler(ICustomerRepository repo)
    : IRequestHandler<GetCustomerListQuery, List<CustomerListItemModel>>
{
    public Task<List<CustomerListItemModel>> Handle(GetCustomerListQuery request, CancellationToken cancellationToken)
        => repo.GetAllAsync(request.Search, request.IsActive, cancellationToken);
}
