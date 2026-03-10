using QBEngineer.Core.Entities;

namespace QBEngineer.Core.Interfaces;

public interface ISystemSettingRepository
{
    Task<List<SystemSetting>> GetAllAsync(CancellationToken ct);
    Task<SystemSetting?> FindByKeyAsync(string key, CancellationToken ct);
    Task AddAsync(SystemSetting setting, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
