using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Customers;

/// <summary>
/// Customer-create command. The first four positional fields preserve the
/// original signature so existing callers (controllers, handler tests) compile
/// unchanged. Phase 3 F3 extends the command with the full-record fields the
/// onboarding form captures at create-time so callers do not need a follow-up
/// PATCH to set credit limit, default tax/currency, or addresses.
/// </summary>
public record CreateCustomerCommand(
    string Name,
    string? CompanyName,
    string? Email,
    string? Phone,
    bool IsTaxExempt = false,
    string? TaxExemptionId = null,
    decimal? CreditLimit = null,
    int? DefaultTaxCodeId = null,
    string? DefaultCurrency = null,
    AddressInput? BillingAddress = null,
    AddressInput? ShippingAddress = null) : IRequest<CustomerListItemModel>;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.TaxExemptionId).MaximumLength(50);
        // If they checked the box, they must give us the cert # — auditors
        // will eventually ask for it and we'd rather not chase the customer.
        RuleFor(x => x.TaxExemptionId)
            .NotEmpty().When(x => x.IsTaxExempt)
            .WithMessage("Tax-exempt customers require an exemption ID on file.");

        // Phase 3 F3 — bounds + ISO checks for the new full-record fields.
        // CreditLimit: any non-negative value up through ~1B keeps obvious
        // typos from sneaking through while not boxing legitimate enterprise
        // shops out. Null is fine (= no credit limit set).
        RuleFor(x => x.CreditLimit)
            .InclusiveBetween(0m, 1_000_000_000m)
            .When(x => x.CreditLimit.HasValue)
            .WithMessage("Credit limit must be between 0 and 1,000,000,000.");

        // ISO 4217: 3 uppercase letters. Whitespace and lowercase rejected
        // rather than silently normalized — small-shop operators are rarely
        // typing 'usd' on purpose, and a hard reject surfaces typos early.
        RuleFor(x => x.DefaultCurrency)
            .Matches(@"^[A-Z]{3}$")
            .When(x => !string.IsNullOrEmpty(x.DefaultCurrency))
            .WithMessage("Currency must be a 3-letter ISO 4217 code (e.g. USD).");

        // Address validation — only enforce required fields if the caller
        // included an address object at all. Either skip the block entirely
        // (omit the property) or send the full set.
        When(x => x.BillingAddress is not null, () =>
        {
            RuleFor(x => x.BillingAddress!.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BillingAddress!.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.BillingAddress!.State).NotEmpty().MaximumLength(100);
            RuleFor(x => x.BillingAddress!.Postal).NotEmpty().MaximumLength(20);
        });
        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress!.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ShippingAddress!.State).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ShippingAddress!.Postal).NotEmpty().MaximumLength(20);
        });
    }
}

public class CreateCustomerHandler(ICustomerRepository repo)
    : IRequestHandler<CreateCustomerCommand, CustomerListItemModel>
{
    public async Task<CustomerListItemModel> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = request.Name,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            IsTaxExempt = request.IsTaxExempt,
            TaxExemptionId = request.TaxExemptionId,
            // F3 — full-record fields written at create time so the GET-by-id
            // round-trip carries the same data the form posted.
            CreditLimit = request.CreditLimit,
            DefaultTaxCodeId = request.DefaultTaxCodeId,
            DefaultCurrency = request.DefaultCurrency,
        };

        if (request.BillingAddress is not null)
            customer.Addresses.Add(MapAddress(request.BillingAddress, AddressType.Billing, label: "Billing"));
        if (request.ShippingAddress is not null)
            customer.Addresses.Add(MapAddress(request.ShippingAddress, AddressType.Shipping, label: "Shipping"));

        await repo.AddAsync(customer, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        return new CustomerListItemModel(
            customer.Id,
            customer.Name,
            customer.CompanyName,
            customer.Email,
            customer.Phone,
            customer.IsActive,
            0, 0,
            customer.CreatedAt);
    }

    private static CustomerAddress MapAddress(AddressInput input, AddressType type, string label) =>
        new()
        {
            Label = label,
            AddressType = type,
            Line1 = input.Street,
            Line2 = input.Line2,
            City = input.City,
            State = input.State,
            PostalCode = input.Postal,
            Country = string.IsNullOrWhiteSpace(input.Country) ? "US" : input.Country!,
            IsDefault = true,
        };
}
