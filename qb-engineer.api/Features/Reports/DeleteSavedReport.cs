using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Reports;

public record DeleteSavedReportCommand(int Id) : IRequest;

public class DeleteSavedReportHandler(
    IReportBuilderRepository repository,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<DeleteSavedReportCommand>
{
    public async Task Handle(DeleteSavedReportCommand request, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        await repository.Delete(request.Id, userId);
    }
}
