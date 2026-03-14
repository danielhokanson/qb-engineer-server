using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.EmployeeProfile;

public record GetProfileCompletenessQuery(int UserId) : IRequest<ProfileCompletenessResponseModel>;

public class GetProfileCompletenessHandler(AppDbContext db) : IRequestHandler<GetProfileCompletenessQuery, ProfileCompletenessResponseModel>
{
    public async Task<ProfileCompletenessResponseModel> Handle(GetProfileCompletenessQuery request, CancellationToken ct)
    {
        var profile = await db.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        var items = new List<ProfileCompletenessItem>
        {
            // Job-assignment blockers — must be complete before user can be assigned work
            new("w4", "W-4 Federal Tax Withholding",
                profile?.W4CompletedAt is not null,
                BlocksJobAssignment: true),

            new("i9", "I-9 Employment Eligibility",
                profile?.I9CompletedAt is not null,
                BlocksJobAssignment: true),

            new("state_withholding", "State Tax Withholding",
                profile?.StateWithholdingCompletedAt is not null,
                BlocksJobAssignment: true),

            new("emergency_contact", "Emergency Contact",
                profile is not null &&
                !string.IsNullOrWhiteSpace(profile.EmergencyContactName) &&
                !string.IsNullOrWhiteSpace(profile.EmergencyContactPhone),
                BlocksJobAssignment: true),

            // Required but do not block job assignment
            new("address", "Home Address",
                profile is not null &&
                !string.IsNullOrWhiteSpace(profile.Street1) &&
                !string.IsNullOrWhiteSpace(profile.City) &&
                !string.IsNullOrWhiteSpace(profile.State) &&
                !string.IsNullOrWhiteSpace(profile.ZipCode),
                BlocksJobAssignment: false),

            new("direct_deposit", "Direct Deposit Authorization",
                profile?.DirectDepositCompletedAt is not null,
                BlocksJobAssignment: false),

            new("workers_comp", "Workers' Comp Acknowledgment",
                profile?.WorkersCompAcknowledgedAt is not null,
                BlocksJobAssignment: false),

            new("handbook", "Employee Handbook Acknowledgment",
                profile?.HandbookAcknowledgedAt is not null,
                BlocksJobAssignment: false),
        };

        var completedCount = items.Count(i => i.IsComplete);
        var canBeAssigned = items.Where(i => i.BlocksJobAssignment).All(i => i.IsComplete);

        return new ProfileCompletenessResponseModel(
            IsComplete: completedCount == items.Count,
            CanBeAssignedJobs: canBeAssigned,
            TotalItems: items.Count,
            CompletedItems: completedCount,
            Items: items);
    }
}
