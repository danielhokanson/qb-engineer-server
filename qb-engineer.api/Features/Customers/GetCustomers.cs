using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Customers;

public record GetCustomersQuery : IRequest<List<CustomerResponseModel>>;

public class GetCustomersHandler(ICustomerRepository repo) : IRequestHandler<GetCustomersQuery, List<CustomerResponseModel>>
{
    public Task<List<CustomerResponseModel>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        => repo.GetAllActiveAsync(cancellationToken);
}
