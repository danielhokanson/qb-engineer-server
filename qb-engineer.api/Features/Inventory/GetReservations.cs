using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetReservationsQuery(int? PartId, int? JobId) : IRequest<List<ReservationResponseModel>>;

public class GetReservationsHandler(IInventoryRepository repo) : IRequestHandler<GetReservationsQuery, List<ReservationResponseModel>>
{
    public Task<List<ReservationResponseModel>> Handle(GetReservationsQuery request, CancellationToken ct)
        => repo.GetReservationsAsync(request.PartId, request.JobId, ct);
}
