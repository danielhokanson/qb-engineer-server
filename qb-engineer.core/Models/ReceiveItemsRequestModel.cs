namespace QBEngineer.Core.Models;

public record ReceiveItemsRequestModel(List<ReceiveLineModel> Lines);
public record ReceiveLineModel(int LineId, decimal Quantity, int? StorageLocationId, string? Notes);
