using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PayStubDeductionResponseModel(
    int Id,
    PayStubDeductionCategory Category,
    string Description,
    decimal Amount);
