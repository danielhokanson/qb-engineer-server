using FluentValidation;
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

public class UpdateCustomerAddressValidator : AbstractValidator<UpdateCustomerAddressCommand>
{
    public UpdateCustomerAddressValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Line2).MaximumLength(200).When(x => x.Line2 is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}

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
