using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetQuoteToCloseReportQuery(DateTimeOffset Start, DateTimeOffset End) : IRequest<List<QuoteToCloseReportItem>>;

public class GetQuoteToCloseHandler(IReportRepository repo)
    : IRequestHandler<GetQuoteToCloseReportQuery, List<QuoteToCloseReportItem>>
{
    public async Task<List<QuoteToCloseReportItem>> Handle(GetQuoteToCloseReportQuery request, CancellationToken ct)
    {
        return await repo.GetQuoteToCloseAsync(request.Start, request.End, ct);
    }
}
