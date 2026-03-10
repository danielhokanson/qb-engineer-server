using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Vendors;

public record GetVendorDropdownQuery : IRequest<List<VendorResponseModel>>;

public class GetVendorDropdownHandler(IVendorRepository repo)
    : IRequestHandler<GetVendorDropdownQuery, List<VendorResponseModel>>
{
    public async Task<List<VendorResponseModel>> Handle(GetVendorDropdownQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllActiveAsync(cancellationToken);
    }
}
