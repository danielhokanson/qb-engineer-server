using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Leads;

public record CreateLeadCommand(CreateLeadRequestModel Data) : IRequest<LeadResponseModel>;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.Data.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.ContactName).MaximumLength(200).When(x => x.Data.ContactName is not null);
        RuleFor(x => x.Data.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Data.Email));
        RuleFor(x => x.Data.Phone).MaximumLength(50).When(x => x.Data.Phone is not null);
    }
}

public class CreateLeadHandler(ILeadRepository repo, IHttpContextAccessor httpContext) : IRequestHandler<CreateLeadCommand, LeadResponseModel>
{
    public async Task<LeadResponseModel> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var lead = new Lead
        {
            CompanyName = data.CompanyName.Trim(),
            ContactName = data.ContactName?.Trim(),
            Email = data.Email?.Trim(),
            Phone = data.Phone?.Trim(),
            Source = data.Source?.Trim(),
            Notes = data.Notes?.Trim(),
            FollowUpDate = data.FollowUpDate,
            CreatedBy = userId,
        };

        await repo.AddAsync(lead, cancellationToken);

        return (await repo.GetByIdAsync(lead.Id, cancellationToken))!;
    }
}
