using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IPaymentRepository
{
    Task<List<PaymentListItemModel>> GetAllAsync(int? customerId, CancellationToken ct);
    Task<Payment?> FindAsync(int id, CancellationToken ct);
    Task<Payment?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task<string> GenerateNextPaymentNumberAsync(CancellationToken ct);
    Task AddAsync(Payment payment, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
