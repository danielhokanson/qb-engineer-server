using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Vendors;

public record GetVendorsQuery(string? Search, bool? IsActive) : IRequest<List<VendorListItemModel>>;

public class GetVendorsHandler(IVendorRepository repo)
    : IRequestHandler<GetVendorsQuery, List<VendorListItemModel>>
{
    public async Task<List<VendorListItemModel>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.Search, request.IsActive, cancellationToken);
    }
}
