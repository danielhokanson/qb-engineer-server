using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Employees;

public record GetEmployeeListQuery(
    string? Search,
    int? TeamId,
    string? Role,
    bool? IsActive,
    int? CallerUserId,
    bool CallerIsAdmin) : IRequest<List<EmployeeListItemResponseModel>>;

public class GetEmployeeListHandler(AppDbContext db, UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetEmployeeListQuery, List<EmployeeListItemResponseModel>>
{
    public async Task<List<EmployeeListItemResponseModel>> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
    {
        var query = db.Users
            .Include(u => u.WorkLocation)
            .AsNoTracking()
            .AsQueryable();

        // Manager restriction: only see users in same team
        if (!request.CallerIsAdmin && request.CallerUserId.HasValue)
        {
            var callerTeamId = await db.Users
                .Where(u => u.Id == request.CallerUserId.Value)
                .Select(u => u.TeamId)
                .FirstOrDefaultAsync(cancellationToken);

            if (callerTeamId.HasValue)
                query = query.Where(u => u.TeamId == callerTeamId);
        }

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        if (request.TeamId.HasValue)
            query = query.Where(u => u.TeamId == request.TeamId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email!.ToLower().Contains(term));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        // Batch-load profiles for job title / department / start date
        var userIds = users.Select(u => u.Id).ToList();
        var profiles = await db.EmployeeProfiles
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        // Batch-load team names
        var teamIds = users.Where(u => u.TeamId.HasValue).Select(u => u.TeamId!.Value).Distinct().ToList();
        var teams = teamIds.Count > 0
            ? await db.ReferenceData
                .AsNoTracking()
                .Where(r => teamIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Label, cancellationToken)
            : new Dictionary<int, string>();

        var result = new List<EmployeeListItemResponseModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Unknown";

            // Filter by role if requested
            if (!string.IsNullOrWhiteSpace(request.Role) && !roles.Contains(request.Role))
                continue;

            profiles.TryGetValue(user.Id, out var profile);
            string? teamName = null;
            if (user.TeamId.HasValue)
                teams.TryGetValue(user.TeamId.Value, out teamName);

            result.Add(new EmployeeListItemResponseModel(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Initials,
                user.AvatarColor,
                user.Email ?? string.Empty,
                profile?.PhoneNumber,
                primaryRole,
                teamName,
                user.TeamId,
                user.IsActive,
                profile?.JobTitle,
                profile?.Department,
                profile?.StartDate,
                user.CreatedAt));
        }

        return result;
    }
}
