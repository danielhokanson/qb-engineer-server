using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreditStatusResponseModel
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal? CreditLimit { get; init; }
    public decimal OpenArBalance { get; init; }
    public decimal PendingOrdersTotal { get; init; }
    public decimal TotalExposure { get; init; }
    public decimal AvailableCredit { get; init; }
    public decimal UtilizationPercent { get; init; }
    public bool IsOnHold { get; init; }
    public string? HoldReason { get; init; }
    public bool IsOverLimit { get; init; }
    public CreditRisk RiskLevel { get; init; }
}
