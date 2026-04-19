namespace QBEngineer.Core.Models;

public record AutoPoSuggestionResponseModel(
    int Id,
    int PartId,
    string PartNumber,
    string? PartDescription,
    int VendorId,
    string VendorName,
    int SuggestedQty,
    DateTimeOffset NeededByDate,
    string Status,
    List<string>? SourceSalesOrderNumbers,
    DateTimeOffset CreatedAt);
