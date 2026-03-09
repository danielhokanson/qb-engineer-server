namespace QBEngineer.Core.Entities;

public class TerminologyEntry : BaseAuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
