using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IUserRepository
{
    Task<List<UserResponseModel>> GetAllActiveAsync(CancellationToken ct);
}
