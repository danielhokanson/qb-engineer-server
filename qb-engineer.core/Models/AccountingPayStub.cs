namespace QBEngineer.Core.Models;

public record AccountingPayStub(
    string ExternalId,
    DateTime PayPeriodStart,
    DateTime PayPeriodEnd,
    DateTime PayDate,
    decimal GrossPay,
    decimal NetPay,
    List<AccountingPayStubDeduction> Deductions);
