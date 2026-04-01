using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Customers;

public record DeleteCustomerCommand(int Id) : IRequest;

public class DeleteCustomerHandler(ICustomerRepository repo)
    : IRequestHandler<DeleteCustomerCommand>
{
    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.Id} not found");

        customer.DeletedAt = DateTimeOffset.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
