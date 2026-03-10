using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.CustomerAddresses;

public record DeleteCustomerAddressCommand(int Id) : IRequest;

public class DeleteCustomerAddressHandler(ICustomerAddressRepository repo)
    : IRequestHandler<DeleteCustomerAddressCommand>
{
    public async Task Handle(DeleteCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Address {request.Id} not found");

        address.DeletedAt = DateTime.UtcNow;
        await repo.SaveChangesAsync(cancellationToken);
    }
}
