using FluentValidation;
using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Vendors;

public record UpdateVendorCommand(
    int Id,
    string? CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    string? PaymentTerms,
    string? Notes,
    bool? IsActive) : IRequest;

public class UpdateVendorValidator : AbstractValidator<UpdateVendorCommand>
{
    public UpdateVendorValidator()
    {
        RuleFor(x => x.CompanyName).MaximumLength(200).When(x => x.CompanyName != null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes != null);
    }
}

public class UpdateVendorHandler(IVendorRepository repo)
    : IRequestHandler<UpdateVendorCommand>
{
    public async Task Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor {request.Id} not found");

        if (request.CompanyName != null) vendor.CompanyName = request.CompanyName;
        if (request.ContactName != null) vendor.ContactName = request.ContactName;
        if (request.Email != null) vendor.Email = request.Email;
        if (request.Phone != null) vendor.Phone = request.Phone;
        if (request.Address != null) vendor.Address = request.Address;
        if (request.City != null) vendor.City = request.City;
        if (request.State != null) vendor.State = request.State;
        if (request.ZipCode != null) vendor.ZipCode = request.ZipCode;
        if (request.Country != null) vendor.Country = request.Country;
        if (request.PaymentTerms != null) vendor.PaymentTerms = request.PaymentTerms;
        if (request.Notes != null) vendor.Notes = request.Notes;
        if (request.IsActive.HasValue) vendor.IsActive = request.IsActive.Value;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
