namespace QBEngineer.Core.Models;

public record AccountingPayStubDeduction(
    string Category,
    string Description,
    decimal Amount);
