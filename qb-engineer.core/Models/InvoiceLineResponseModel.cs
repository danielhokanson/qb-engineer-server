namespace QBEngineer.Core.Models;

public record InvoiceLineResponseModel(
    int Id,
    int? PartId,
    string? PartNumber,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int LineNumber);
