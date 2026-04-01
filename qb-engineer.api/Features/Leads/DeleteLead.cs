using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Leads;

public sealed record DeleteLeadCommand(int Id) : IRequest;

public sealed class DeleteLeadHandler(ILeadRepository repo)
    : IRequestHandler<DeleteLeadCommand>
{
    public async Task Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead {request.Id} not found");

        if (lead.Status == LeadStatus.Converted)
            throw new InvalidOperationException("Converted leads cannot be deleted.");

        lead.DeletedAt = DateTimeOffset.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
