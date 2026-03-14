namespace QBEngineer.Core.Entities;

public class CompanyLocation : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "US";
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
