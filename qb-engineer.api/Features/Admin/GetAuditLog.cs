using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record GetAuditLogQuery(
    int? UserId,
    string? Action,
    string? EntityType,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page = 1,
    int PageSize = 25) : IRequest<PaginatedResult<AuditLogEntryResponseModel>>;

public record PaginatedResult<T>(List<T> Data, int Page, int PageSize, int TotalCount, int TotalPages);

public class GetAuditLogHandler(AppDbContext db) : IRequestHandler<GetAuditLogQuery, PaginatedResult<AuditLogEntryResponseModel>>
{
    public async Task<PaginatedResult<AuditLogEntryResponseModel>> Handle(GetAuditLogQuery request, CancellationToken ct)
    {
        var query = db.AuditLogEntries.AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);
        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action == request.Action);
        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);
        if (request.From.HasValue)
            query = query.Where(a => a.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(a => a.CreatedAt <= request.To.Value);

        var totalCount = await query.CountAsync(ct);

        var entries = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(db.Users, a => a.UserId, u => u.Id, (a, u) => new AuditLogEntryResponseModel(
                a.Id,
                a.UserId,
                (u.FirstName + " " + u.LastName).Trim(),
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Details,
                a.IpAddress,
                a.CreatedAt))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        return new PaginatedResult<AuditLogEntryResponseModel>(entries, request.Page, request.PageSize, totalCount, totalPages);
    }
}
