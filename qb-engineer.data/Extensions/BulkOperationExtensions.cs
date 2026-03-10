using EFCore.BulkExtensions;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Extensions;

public static class BulkOperationExtensions
{
    public static async Task BulkSoftDeleteAsync<T>(this DbContext context, IList<T> entities, string deletedBy) where T : BaseAuditableEntity
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.DeletedAt = now;
            entity.DeletedBy = deletedBy;
        }
        await context.BulkUpdateAsync(entities);
    }
}
