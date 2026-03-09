using FluentValidation;
using MediatR;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Customers;

public record UpdateCustomerCommand(
    int Id,
    string? Name,
    string? CompanyName,
    string? Email,
    string? Phone,
    bool? IsActive) : IRequest;

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.CompanyName).MaximumLength(200).When(x => x.CompanyName is not null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
    }
}

public class UpdateCustomerHandler(ICustomerRepository repo)
    : IRequestHandler<UpdateCustomerCommand>
{
    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer {request.Id} not found");

        if (request.Name is not null) customer.Name = request.Name;
        if (request.CompanyName is not null) customer.CompanyName = request.CompanyName;
        if (request.Email is not null) customer.Email = request.Email;
        if (request.Phone is not null) customer.Phone = request.Phone;
        if (request.IsActive.HasValue) customer.IsActive = request.IsActive.Value;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
