using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record AccountingCustomer(
    string ExternalId,
    string Name,
    string? Email,
    string? Phone,
    string? CompanyName,
    decimal Balance);

public record AccountingDocument(
    AccountingDocumentType Type,
    string CustomerExternalId,
    List<AccountingLineItem> LineItems,
    string? RefNumber,
    decimal Amount,
    DateTime Date);

public record AccountingLineItem(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string? ItemExternalId);

public record AccountingPayment(
    string ExternalId,
    decimal Amount,
    DateTime Date,
    string? Method);

public record AccountingTimeActivity(
    string EmployeeExternalId,
    string? CustomerExternalId,
    decimal Hours,
    DateTime Date,
    string? Description,
    string? ServiceItemExternalId);

public record AccountingSyncStatus(
    bool Connected,
    DateTime? LastSyncAt,
    int QueueDepth,
    int FailedCount);
