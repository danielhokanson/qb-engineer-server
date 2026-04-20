using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IIntegrationOutboxService
{
    Task<IntegrationOutboxEntry> EnqueueEmailAsync(
        string operationKey,
        EmailMessage message,
        string? entityType = null,
        int? entityId = null,
        CancellationToken ct = default);

    Task<IntegrationOutboxEntry> EnqueueAsync(
        IntegrationProvider provider,
        string operationKey,
        string payload,
        string? entityType = null,
        int? entityId = null,
        int maxAttempts = 5,
        CancellationToken ct = default);
}
