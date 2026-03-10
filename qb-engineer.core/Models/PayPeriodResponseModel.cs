namespace QBEngineer.Core.Models;

public record PayPeriodResponseModel(
    string Type,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int DaysRemaining);
