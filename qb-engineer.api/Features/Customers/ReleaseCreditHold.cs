using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record ReleaseCreditHoldCommand(int CustomerId) : IRequest;

public class ReleaseCreditHoldHandler(AppDbContext db) : IRequestHandler<ReleaseCreditHoldCommand>
{
    public async Task Handle(ReleaseCreditHoldCommand request, CancellationToken ct)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        customer.IsOnCreditHold = false;
        customer.CreditHoldReason = null;
        customer.CreditHoldAt = null;
        customer.CreditHoldById = null;
        customer.LastCreditReviewDate = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
