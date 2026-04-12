using MediatR;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Projects;

public record RecalculateProjectTotalsCommand(int Id) : IRequest;

public class RecalculateProjectTotalsHandler(IProjectAccountingService projectService) : IRequestHandler<RecalculateProjectTotalsCommand>
{
    public async Task Handle(RecalculateProjectTotalsCommand command, CancellationToken cancellationToken)
    {
        await projectService.RecalculateProjectTotalsAsync(command.Id, cancellationToken);
    }
}
