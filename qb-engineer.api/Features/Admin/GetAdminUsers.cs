using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record GetAdminUsersQuery : IRequest<List<AdminUserResponseModel>>;

public class GetAdminUsersHandler(AppDbContext db, UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetAdminUsersQuery, List<AdminUserResponseModel>>
{
    public async Task<List<AdminUserResponseModel>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await db.Users.OrderBy(u => u.FirstName).ToListAsync(cancellationToken);

        var result = new List<AdminUserResponseModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new AdminUserResponseModel(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.Initials,
                user.AvatarColor,
                user.IsActive,
                roles.ToArray(),
                user.CreatedAt));
        }

        return result;
    }
}
