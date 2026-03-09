using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ICustomerRepository
{
    Task<List<CustomerResponseModel>> GetAllActiveAsync(CancellationToken ct);
}
