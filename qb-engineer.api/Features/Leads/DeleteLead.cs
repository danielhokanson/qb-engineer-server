using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;
using QBEngineer.Data.Extensions;

namespace QBEngineer.Api.Features.Leads;

public sealed record DeleteLeadCommand(int Id) : IRequest;

public sealed class DeleteLeadHandler(ILeadRepository repo, AppDbContext db, IClock clock)
    : IRequestHandler<DeleteLeadCommand>
{
    public async Task Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead {request.Id} not found");

        if (lead.Status == LeadStatus.Converted)
            throw new InvalidOperationException("Converted leads cannot be deleted.");

        lead.DeletedAt = clock.UtcNow;
        // DeletedBy auto-stamped by AppDbContext.SetTimestamps.

        db.LogActivityAt(
            "deleted",
            $"Deleted lead: {lead.CompanyName}{(string.IsNullOrEmpty(lead.ContactName) ? "" : $" — {lead.ContactName}")}",
            ("Lead", lead.Id));

        await repo.SaveChangesAsync(cancellationToken);
    }
}
