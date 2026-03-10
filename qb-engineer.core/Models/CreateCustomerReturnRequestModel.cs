namespace QBEngineer.Core.Models;

public record CreateCustomerReturnRequestModel(
    int CustomerId,
    int OriginalJobId,
    string Reason,
    string? Notes,
    DateTime ReturnDate,
    bool CreateReworkJob);
