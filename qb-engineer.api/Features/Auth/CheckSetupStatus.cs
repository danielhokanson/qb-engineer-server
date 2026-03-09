using MediatR;
using Microsoft.AspNetCore.Identity;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record SetupStatusResponseModel(bool SetupRequired);

public record CheckSetupStatusQuery : IRequest<SetupStatusResponseModel>;

public class CheckSetupStatusHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<CheckSetupStatusQuery, SetupStatusResponseModel>
{
    public Task<SetupStatusResponseModel> Handle(CheckSetupStatusQuery request, CancellationToken cancellationToken)
    {
        var anyUsers = userManager.Users.Any();
        return Task.FromResult(new SetupStatusResponseModel(!anyUsers));
    }
}
