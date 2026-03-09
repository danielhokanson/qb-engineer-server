namespace QBEngineer.Core.Models;

public record AccountingPayment(
    string ExternalId,
    decimal Amount,
    DateTime Date,
    string? Method);
