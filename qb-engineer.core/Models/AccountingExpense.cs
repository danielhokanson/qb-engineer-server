namespace QBEngineer.Core.Models;

public record AccountingExpense(
    string? VendorExternalId,
    string? CustomerExternalId,
    decimal Amount,
    DateTime Date,
    string? Description,
    string? Category,
    string? RefNumber);
