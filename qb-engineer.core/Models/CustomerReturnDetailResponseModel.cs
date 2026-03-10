namespace QBEngineer.Core.Models;

public record CustomerReturnDetailResponseModel(
    int Id,
    string ReturnNumber,
    int CustomerId,
    string CustomerName,
    int OriginalJobId,
    string OriginalJobNumber,
    string OriginalJobTitle,
    int? ReworkJobId,
    string? ReworkJobNumber,
    string Status,
    string Reason,
    string? Notes,
    DateTime ReturnDate,
    int? InspectedById,
    string? InspectedByName,
    DateTime? InspectedAt,
    string? InspectionNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
