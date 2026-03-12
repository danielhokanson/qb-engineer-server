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

        // Batch-load scan identifier types per user
        var scanTypes = await db.Set<QBEngineer.Core.Entities.UserScanIdentifier>()
            .Where(s => s.IsActive && s.DeletedAt == null)
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                HasRfid = g.Any(s => s.IdentifierType == "rfid" || s.IdentifierType == "nfc"),
                HasBarcode = g.Any(s => s.IdentifierType == "barcode"),
            })
            .ToDictionaryAsync(x => x.UserId, cancellationToken);

        var result = new List<AdminUserResponseModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var hasPassword = await userManager.HasPasswordAsync(user);
            var hasPendingToken = user.SetupToken != null
                && user.SetupTokenExpiresAt.HasValue
                && user.SetupTokenExpiresAt.Value > DateTime.UtcNow;
            scanTypes.TryGetValue(user.Id, out var scan);
            result.Add(new AdminUserResponseModel(
                user.Id,
                user.Email!,
                user.FirstName,
                user.LastName,
                user.Initials,
                user.AvatarColor,
                user.IsActive,
                roles.ToArray(),
                user.CreatedAt,
                hasPassword,
                hasPendingToken,
                scan?.HasRfid ?? false,
                scan?.HasBarcode ?? false));
        }

        return result;
    }
}
