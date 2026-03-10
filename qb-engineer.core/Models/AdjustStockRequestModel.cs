namespace QBEngineer.Core.Models;

public record AdjustStockRequestModel(
    int BinContentId,
    int NewQuantity,
    string Reason,
    string? Notes);
