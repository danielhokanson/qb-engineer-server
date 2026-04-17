using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Announcements;

public record GetAnnouncementAcknowledgmentsQuery(int AnnouncementId) : IRequest<List<AnnouncementAcknowledgmentResponseModel>>;

public class GetAnnouncementAcknowledgmentsHandler(AppDbContext db) : IRequestHandler<GetAnnouncementAcknowledgmentsQuery, List<AnnouncementAcknowledgmentResponseModel>>
{
    public async Task<List<AnnouncementAcknowledgmentResponseModel>> Handle(GetAnnouncementAcknowledgmentsQuery request, CancellationToken ct)
    {
        return await db.AnnouncementAcknowledgments
            .AsNoTracking()
            .Where(a => a.AnnouncementId == request.AnnouncementId)
            .Join(db.Users, a => a.UserId, u => u.Id, (a, u) => new AnnouncementAcknowledgmentResponseModel(
                u.Id,
                (u.FirstName + " " + u.LastName).Trim(),
                a.AcknowledgedAt))
            .OrderBy(a => a.AcknowledgedAt)
            .ToListAsync(ct);
    }
}
