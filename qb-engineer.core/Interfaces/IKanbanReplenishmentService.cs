using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IKanbanReplenishmentService
{
    Task<KanbanCard> CreateCardAsync(CreateKanbanCardRequestModel request, CancellationToken ct);
    Task<KanbanCard> UpdateCardAsync(int cardId, UpdateKanbanCardRequestModel request, CancellationToken ct);
    Task TriggerReplenishmentAsync(int cardId, KanbanTriggerType triggerType, int? triggeredByUserId, CancellationToken ct);
    Task ConfirmReplenishmentAsync(int cardId, decimal fulfilledQuantity, CancellationToken ct);
    Task<IReadOnlyList<KanbanCard>> GetCardsByWorkCenterAsync(int workCenterId, CancellationToken ct);
    Task<IReadOnlyList<KanbanCard>> GetTriggeredCardsAsync(CancellationToken ct);
    Task CalculateOptimalBinQuantityAsync(int cardId, CancellationToken ct);
}
