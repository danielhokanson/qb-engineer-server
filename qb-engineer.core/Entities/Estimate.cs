using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Estimate : BaseAuditableEntity
{
    public int CustomerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal EstimatedAmount { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public EstimateStatus Status { get; set; } = EstimateStatus.Draft;
    public string? Notes { get; set; }
    public int? AssignedToId { get; set; }

    public int? ConvertedToQuoteId { get; set; }
    public DateTimeOffset? ConvertedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Quote? ConvertedToQuote { get; set; }
}
