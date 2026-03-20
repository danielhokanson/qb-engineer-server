using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
    Task<bool> TestConnectionAsync(CancellationToken ct);
}
