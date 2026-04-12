using System.Security.Claims;

using MediatR;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.KanbanReplenishment;

public record TriggerKanbanReplenishmentCommand(int Id, TriggerKanbanReplenishmentRequestModel Request, int? UserId) : IRequest;

public class TriggerKanbanReplenishmentHandler(IKanbanReplenishmentService kanbanService) : IRequestHandler<TriggerKanbanReplenishmentCommand>
{
    public async Task Handle(TriggerKanbanReplenishmentCommand command, CancellationToken cancellationToken)
    {
        var triggerType = Enum.TryParse<KanbanTriggerType>(command.Request.TriggerType, true, out var parsed)
            ? parsed
            : KanbanTriggerType.Manual;

        await kanbanService.TriggerReplenishmentAsync(command.Id, triggerType, command.UserId, cancellationToken);
    }
}
