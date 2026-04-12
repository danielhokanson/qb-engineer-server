using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.KanbanReplenishment;

public record CreateKanbanCardCommand(CreateKanbanCardRequestModel Request) : IRequest<KanbanCardResponseModel>;

public class CreateKanbanCardHandler(IKanbanReplenishmentService kanbanService) : IRequestHandler<CreateKanbanCardCommand, KanbanCardResponseModel>
{
    public async Task<KanbanCardResponseModel> Handle(CreateKanbanCardCommand command, CancellationToken cancellationToken)
    {
        var card = await kanbanService.CreateCardAsync(command.Request, cancellationToken);

        return new KanbanCardResponseModel
        {
            Id = card.Id,
            CardNumber = card.CardNumber,
            PartId = card.PartId,
            WorkCenterId = card.WorkCenterId,
            BinQuantity = card.BinQuantity,
            NumberOfBins = card.NumberOfBins,
            Status = card.Status.ToString(),
            SupplySource = card.SupplySource.ToString(),
            IsActive = card.IsActive,
        };
    }
}
