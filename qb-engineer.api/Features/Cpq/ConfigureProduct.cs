using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Cpq;

public record ConfigureProductCommand(ConfigureProductRequestModel Request) : IRequest<CpqResult>;

public class ConfigureProductHandler(ICpqService cpqService) : IRequestHandler<ConfigureProductCommand, CpqResult>
{
    public async Task<CpqResult> Handle(ConfigureProductCommand command, CancellationToken cancellationToken)
    {
        return await cpqService.ConfigureAsync(
            command.Request.ConfiguratorId,
            command.Request.Selections,
            cancellationToken);
    }
}
