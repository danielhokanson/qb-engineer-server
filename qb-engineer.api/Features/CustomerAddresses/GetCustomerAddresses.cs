using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.CustomerAddresses;

public record GetCustomerAddressesQuery(int CustomerId) : IRequest<List<CustomerAddressResponseModel>>;

public class GetCustomerAddressesHandler(ICustomerAddressRepository repo)
    : IRequestHandler<GetCustomerAddressesQuery, List<CustomerAddressResponseModel>>
{
    public async Task<List<CustomerAddressResponseModel>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetByCustomerAsync(request.CustomerId, cancellationToken);
    }
}
