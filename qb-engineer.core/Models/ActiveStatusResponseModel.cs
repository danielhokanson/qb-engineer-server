namespace QBEngineer.Core.Models;

public record ActiveStatusResponseModel(
    StatusEntryResponseModel? WorkflowStatus,
    List<StatusEntryResponseModel> ActiveHolds);
