using System.Security.Claims;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record PlaceCreditHoldCommand(int CustomerId, string Reason) : IRequest;

public class PlaceCreditHoldValidator : AbstractValidator<PlaceCreditHoldCommand>
{
    public PlaceCreditHoldValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class PlaceCreditHoldHandler(AppDbContext db, IHttpContextAccessor httpContext) : IRequestHandler<PlaceCreditHoldCommand>
{
    public async Task Handle(PlaceCreditHoldCommand request, CancellationToken ct)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, ct)
            ?? throw new KeyNotFoundException($"Customer {request.CustomerId} not found");

        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        customer.IsOnCreditHold = true;
        customer.CreditHoldReason = request.Reason;
        customer.CreditHoldAt = DateTimeOffset.UtcNow;
        customer.CreditHoldById = userId;

        await db.SaveChangesAsync(ct);
    }
}
