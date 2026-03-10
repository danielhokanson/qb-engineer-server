namespace QBEngineer.Core.Models;

public record UninvoicedJobResponseModel(
    int JobId,
    string JobNumber,
    string Title,
    string? CustomerName,
    DateTime CompletedDate,
    int? CustomerId);
