using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Users;

public record GetUsersQuery : IRequest<List<UserResponseModel>>;

public class GetUsersHandler(IUserRepository repo) : IRequestHandler<GetUsersQuery, List<UserResponseModel>>
{
    public Task<List<UserResponseModel>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => repo.GetAllActiveAsync(cancellationToken);
}
