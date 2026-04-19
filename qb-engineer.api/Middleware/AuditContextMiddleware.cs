using System.Security.Claims;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Middleware;

/// <summary>
/// Sets the CurrentUserId on AppDbContext for automatic audit logging.
/// Must run after UseAuthentication/UseAuthorization.
/// </summary>
public class AuditContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            db.CurrentUserId = userId;
        }

        await next(context);
    }
}
