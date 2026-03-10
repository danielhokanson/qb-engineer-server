using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<List<UserResponseModel>> GetAllActiveAsync(CancellationToken ct)
    {
        return await db.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .Select(u => new UserResponseModel(
                u.Id,
                u.Initials ?? "??",
                (u.FirstName + " " + u.LastName).Trim(),
                u.AvatarColor ?? "#94a3b8"))
            .ToListAsync(ct);
    }

    public async Task<List<UserResponseModel>> FindByNamesAsync(IEnumerable<string> names, CancellationToken ct)
    {
        var nameList = names.Select(n => n.ToLower()).ToList();
        return await db.Users
            .Where(u => u.IsActive)
            .Where(u => nameList.Contains(u.FirstName!.ToLower())
                     || nameList.Contains((u.FirstName + " " + u.LastName).Trim().ToLower().Replace(" ", "")))
            .Select(u => new UserResponseModel(
                u.Id,
                u.Initials ?? "??",
                (u.FirstName + " " + u.LastName).Trim(),
                u.AvatarColor ?? "#94a3b8"))
            .ToListAsync(ct);
    }
}
