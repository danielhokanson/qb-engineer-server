using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetAtpForPartQuery(int PartId, decimal Quantity) : IRequest<AtpResult>;

public class GetAtpForPartHandler(IAtpService atpService) : IRequestHandler<GetAtpForPartQuery, AtpResult>
{
    public async Task<AtpResult> Handle(GetAtpForPartQuery request, CancellationToken cancellationToken)
    {
        return await atpService.CalculateAtpAsync(request.PartId, request.Quantity, cancellationToken);
    }
}
