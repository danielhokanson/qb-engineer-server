using FluentValidation;
using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Shipments;

public record ValidateShippingAddressCommand(ValidateAddressRequestModel Request) : IRequest<AddressValidationResponseModel>;

public class ValidateShippingAddressValidator : AbstractValidator<ValidateShippingAddressCommand>
{
    public ValidateShippingAddressValidator()
    {
        RuleFor(x => x.Request.Street).NotEmpty();
        RuleFor(x => x.Request.City).NotEmpty();
        RuleFor(x => x.Request.State).NotEmpty();
        RuleFor(x => x.Request.Zip).NotEmpty();
        RuleFor(x => x.Request.Country).NotEmpty();
    }
}

public class ValidateShippingAddressHandler(IShippingService shippingService)
    : IRequestHandler<ValidateShippingAddressCommand, AddressValidationResponseModel>
{
    public async Task<AddressValidationResponseModel> Handle(ValidateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        var address = new ShippingAddress(
            string.Empty, // Name not needed for validation
            request.Request.Street,
            request.Request.City,
            request.Request.State,
            request.Request.Zip,
            request.Request.Country);

        return await shippingService.ValidateAddressAsync(address, cancellationToken);
    }
}
