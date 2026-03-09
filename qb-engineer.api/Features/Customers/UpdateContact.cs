using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record UpdateContactCommand(
    int CustomerId,
    int ContactId,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Role,
    bool? IsPrimary) : IRequest<ContactResponseModel>;

public class UpdateContactValidator : AbstractValidator<UpdateContactCommand>
{
    public UpdateContactValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => x.FirstName is not null);
        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName is not null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
        RuleFor(x => x.Role).MaximumLength(50).When(x => x.Role is not null);
    }
}

public class UpdateContactHandler(AppDbContext db)
    : IRequestHandler<UpdateContactCommand, ContactResponseModel>
{
    public async Task<ContactResponseModel> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await db.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact {request.ContactId} not found");

        if (request.FirstName is not null) contact.FirstName = request.FirstName;
        if (request.LastName is not null) contact.LastName = request.LastName;
        if (request.Email is not null) contact.Email = request.Email;
        if (request.Phone is not null) contact.Phone = request.Phone;
        if (request.Role is not null) contact.Role = request.Role;
        if (request.IsPrimary.HasValue) contact.IsPrimary = request.IsPrimary.Value;

        await db.SaveChangesAsync(cancellationToken);

        return new ContactResponseModel(
            contact.Id, contact.FirstName, contact.LastName,
            contact.Email, contact.Phone, contact.Role, contact.IsPrimary);
    }
}
