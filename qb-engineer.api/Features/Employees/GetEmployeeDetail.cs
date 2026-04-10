using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Employees;

public record GetEmployeeDetailQuery(int EmployeeId) : IRequest<EmployeeDetailResponseModel?>;

public class GetEmployeeDetailHandler(AppDbContext db, UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetEmployeeDetailQuery, EmployeeDetailResponseModel?>
{
    public async Task<EmployeeDetailResponseModel?> Handle(GetEmployeeDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.WorkLocation)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.EmployeeId, cancellationToken);

        if (user == null) return null;

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Unknown";

        var profile = await db.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);

        // Team name
        string? teamName = null;
        if (user.TeamId.HasValue)
        {
            teamName = await db.ReferenceData
                .AsNoTracking()
                .Where(r => r.Id == user.TeamId.Value)
                .Select(r => r.Label)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Scan identifiers
        var scanTypes = await db.Set<QBEngineer.Core.Entities.UserScanIdentifier>()
            .Where(s => s.UserId == user.Id && s.IsActive && s.DeletedAt == null)
            .Select(s => s.IdentifierType)
            .ToListAsync(cancellationToken);

        // Compliance summary
        var complianceItems = new[]
        {
            profile?.W4CompletedAt is not null,
            profile?.I9CompletedAt is not null,
            profile?.StateWithholdingCompletedAt is not null,
            profile is not null &&
                !string.IsNullOrWhiteSpace(profile.EmergencyContactName) &&
                !string.IsNullOrWhiteSpace(profile.EmergencyContactPhone),
            profile is not null &&
                !string.IsNullOrWhiteSpace(profile.Street1) &&
                !string.IsNullOrWhiteSpace(profile.City) &&
                !string.IsNullOrWhiteSpace(profile.State) &&
                !string.IsNullOrWhiteSpace(profile.ZipCode),
            profile?.DirectDepositCompletedAt is not null,
            profile?.WorkersCompAcknowledgedAt is not null,
            profile?.HandbookAcknowledgedAt is not null,
        };

        return new EmployeeDetailResponseModel(
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
            user.CreatedAt,
            user.WorkLocationId,
            user.WorkLocation?.Name,
            !string.IsNullOrEmpty(user.PinHash),
            scanTypes.Any(t => t == "rfid" || t == "nfc"),
            scanTypes.Any(t => t == "barcode"),
            profile?.PersonalEmail,
            profile?.Street1,
            profile?.Street2,
            profile?.City,
            profile?.State,
            profile?.ZipCode,
            profile?.EmergencyContactName,
            profile?.EmergencyContactPhone,
            profile?.EmergencyContactRelationship,
            complianceItems.Count(c => c),
            complianceItems.Length);
    }
}
