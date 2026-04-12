using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Quality;

public record GetCopqParetoQuery(DateOnly StartDate, DateOnly EndDate) : IRequest<IReadOnlyList<CopqParetoItemResponseModel>>;

public class GetCopqParetoHandler(ICopqService copqService)
    : IRequestHandler<GetCopqParetoQuery, IReadOnlyList<CopqParetoItemResponseModel>>
{
    public async Task<IReadOnlyList<CopqParetoItemResponseModel>> Handle(
        GetCopqParetoQuery request, CancellationToken cancellationToken)
    {
        return await copqService.GetParetoByDefectAsync(request.StartDate, request.EndDate, cancellationToken);
    }
}
