using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.CustomerAddresses;

public record UpdateCustomerAddressCommand(
    int Id,
    string Label,
    string AddressType,
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault) : IRequest;

public class UpdateCustomerAddressHandler(ICustomerAddressRepository repo)
    : IRequestHandler<UpdateCustomerAddressCommand>
{
    public async Task Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Address {request.Id} not found");

        address.Label = request.Label;
        address.AddressType = Enum.Parse<AddressType>(request.AddressType, true);
        address.Line1 = request.Line1;
        address.Line2 = request.Line2;
        address.City = request.City;
        address.State = request.State;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country;
        address.IsDefault = request.IsDefault;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
