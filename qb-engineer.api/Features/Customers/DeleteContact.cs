using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record DeleteContactCommand(int CustomerId, int ContactId) : IRequest;

public class DeleteContactHandler(AppDbContext db)
    : IRequestHandler<DeleteContactCommand>
{
    public async Task Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await db.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact {request.ContactId} not found");

        contact.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
