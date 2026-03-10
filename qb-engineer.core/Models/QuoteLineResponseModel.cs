namespace QBEngineer.Core.Models;

public record QuoteLineResponseModel(
    int Id,
    int? PartId,
    string? PartNumber,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int LineNumber,
    string? Notes);
