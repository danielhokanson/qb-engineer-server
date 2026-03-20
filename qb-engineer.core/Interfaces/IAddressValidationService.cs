using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IAddressValidationService
{
    Task<AddressValidationResponseModel> ValidateAsync(ValidateAddressRequestModel request, CancellationToken ct);
    Task<bool> TestConnectionAsync(CancellationToken ct);
}
