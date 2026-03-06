namespace QbEngineer.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}
