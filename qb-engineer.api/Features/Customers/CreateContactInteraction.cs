using System.Security.Claims;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Customers;

public record CreateContactInteractionCommand(
    int CustomerId,
    int? ContactId,
    string Type,
    string Subject,
    string? Body,
    DateTimeOffset InteractionDate,
    int? DurationMinutes) : IRequest<ContactInteractionResponseModel>;

public class CreateContactInteractionValidator : AbstractValidator<CreateContactInteractionCommand>
{
    public CreateContactInteractionValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).MaximumLength(4000);
        RuleFor(x => x.Type).NotEmpty().Must(t => Enum.TryParse<InteractionType>(t, true, out _))
            .WithMessage("Invalid interaction type");
    }
}

public class CreateContactInteractionHandler(AppDbContext db, IHttpContextAccessor httpContext)
    : IRequestHandler<CreateContactInteractionCommand, ContactInteractionResponseModel>
{
    public async Task<ContactInteractionResponseModel> Handle(
        CreateContactInteractionCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // If no contactId, get the primary contact for the customer
        int contactId;
        if (request.ContactId.HasValue)
        {
            var contact = await db.Contacts
                .FirstOrDefaultAsync(c => c.Id == request.ContactId.Value && c.CustomerId == request.CustomerId, cancellationToken)
                ?? throw new KeyNotFoundException($"Contact {request.ContactId.Value} not found for customer {request.CustomerId}");
            contactId = contact.Id;
        }
        else
        {
            var primaryContact = await db.Contacts
                .Where(c => c.CustomerId == request.CustomerId)
                .OrderByDescending(c => c.IsPrimary)
                .ThenBy(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"No contacts found for customer {request.CustomerId}");
            contactId = primaryContact.Id;
        }

        var interaction = new ContactInteraction
        {
            ContactId = contactId,
            UserId = userId,
            Type = Enum.Parse<InteractionType>(request.Type, true),
            Subject = request.Subject,
            Body = request.Body,
            InteractionDate = request.InteractionDate,
            DurationMinutes = request.DurationMinutes,
        };

        db.ContactInteractions.Add(interaction);
        await db.SaveChangesAsync(cancellationToken);

        // Load navigation for response
        var contactInfo = await db.Contacts
            .Where(c => c.Id == contactId)
            .Select(c => new { c.FirstName, c.LastName })
            .FirstAsync(cancellationToken);

        var userInfo = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstAsync(cancellationToken);

        return new ContactInteractionResponseModel(
            interaction.Id,
            interaction.ContactId,
            $"{contactInfo.LastName}, {contactInfo.FirstName}",
            interaction.UserId,
            $"{userInfo.LastName}, {userInfo.FirstName}",
            interaction.Type.ToString(),
            interaction.Subject,
            interaction.Body,
            interaction.InteractionDate,
            interaction.DurationMinutes,
            interaction.CreatedAt);
    }
}
