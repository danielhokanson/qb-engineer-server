using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.UserPreferences;

public record GetUserPreferencesQuery(int UserId) : IRequest<List<UserPreferenceResponseModel>>;

public class GetUserPreferencesHandler(IUserPreferenceRepository repo)
    : IRequestHandler<GetUserPreferencesQuery, List<UserPreferenceResponseModel>>
{
    public Task<List<UserPreferenceResponseModel>> Handle(
        GetUserPreferencesQuery request, CancellationToken cancellationToken)
        => repo.GetByUserIdAsync(request.UserId, cancellationToken);
}
