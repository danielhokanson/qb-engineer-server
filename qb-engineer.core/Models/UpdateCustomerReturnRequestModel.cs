namespace QBEngineer.Core.Models;

public record UpdateCustomerReturnRequestModel(
    string? Reason,
    string? Notes,
    string? InspectionNotes);
