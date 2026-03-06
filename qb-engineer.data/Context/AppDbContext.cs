using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QbEngineer.Core.Entities;

namespace QbEngineer.Data.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply snake_case naming convention for all tables and columns
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName()!));

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
        }

        // Global query filter for soft delete on all BaseAuditableEntity types
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var deletedAtProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseAuditableEntity.DeletedAt));
            var nullConstant = System.Linq.Expressions.Expression.Constant(null, typeof(DateTime?));
            var filter = System.Linq.Expressions.Expression.Equal(deletedAtProperty, nullConstant);
            var lambda = System.Linq.Expressions.Expression.Lambda(filter, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])
                ? "_" + c
                : c.ToString()))
            .ToLowerInvariant();
    }
}
