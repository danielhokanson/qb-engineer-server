using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Quality;

public record GetCopqTrendQuery(int Months) : IRequest<IReadOnlyList<CopqTrendPointResponseModel>>;

public class GetCopqTrendHandler(ICopqService copqService)
    : IRequestHandler<GetCopqTrendQuery, IReadOnlyList<CopqTrendPointResponseModel>>
{
    public async Task<IReadOnlyList<CopqTrendPointResponseModel>> Handle(
        GetCopqTrendQuery request, CancellationToken cancellationToken)
    {
        return await copqService.GetTrendAsync(request.Months, cancellationToken);
    }
}
