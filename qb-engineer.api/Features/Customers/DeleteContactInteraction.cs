using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record DeleteContactInteractionCommand(int CustomerId, int InteractionId) : IRequest;

public class DeleteContactInteractionHandler(AppDbContext db)
    : IRequestHandler<DeleteContactInteractionCommand>
{
    public async Task Handle(DeleteContactInteractionCommand request, CancellationToken cancellationToken)
    {
        var interaction = await db.ContactInteractions
            .Include(ci => ci.Contact)
            .FirstOrDefaultAsync(ci => ci.Id == request.InteractionId
                && ci.Contact.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Interaction {request.InteractionId} not found for customer {request.CustomerId}");

        db.ContactInteractions.Remove(interaction);
        await db.SaveChangesAsync(cancellationToken);
    }
}
