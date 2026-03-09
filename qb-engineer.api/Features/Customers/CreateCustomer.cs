using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Customers;

public record CreateCustomerCommand(
    string Name,
    string? CompanyName,
    string? Email,
    string? Phone) : IRequest<CustomerListItemModel>;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
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
        };

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
}
