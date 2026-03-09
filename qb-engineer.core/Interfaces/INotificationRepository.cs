using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface INotificationRepository
{
    Task<List<NotificationResponseModel>> GetByUserIdAsync(int userId, CancellationToken ct);
    Task<Notification?> FindAsync(int id, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
    Task MarkAllReadAsync(int userId, CancellationToken ct);
    Task DismissAllAsync(int userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
