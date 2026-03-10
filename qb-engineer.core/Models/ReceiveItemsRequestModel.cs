namespace QBEngineer.Core.Models;

public record ReceiveItemsRequestModel(List<ReceiveLineModel> Lines);
public record ReceiveLineModel(int LineId, int Quantity, int? StorageLocationId, string? Notes);
