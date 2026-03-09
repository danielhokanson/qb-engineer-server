using FluentValidation;
using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Leads;

public record UpdateLeadCommand(int Id, UpdateLeadRequestModel Data) : IRequest<LeadResponseModel>;

public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.CompanyName).MaximumLength(200).When(x => x.Data.CompanyName is not null);
        RuleFor(x => x.Data.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Data.Email));
        RuleFor(x => x.Data.Phone).MaximumLength(50).When(x => x.Data.Phone is not null);
    }
}

public class UpdateLeadHandler(ILeadRepository repo) : IRequestHandler<UpdateLeadCommand, LeadResponseModel>
{
    public async Task<LeadResponseModel> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Lead not found.");

        var data = request.Data;

        if (data.CompanyName is not null) lead.CompanyName = data.CompanyName.Trim();
        if (data.ContactName is not null) lead.ContactName = data.ContactName.Trim();
        if (data.Email is not null) lead.Email = data.Email.Trim();
        if (data.Phone is not null) lead.Phone = data.Phone.Trim();
        if (data.Source is not null) lead.Source = data.Source.Trim();
        if (data.Status.HasValue) lead.Status = data.Status.Value;
        if (data.Notes is not null) lead.Notes = data.Notes.Trim();
        if (data.FollowUpDate.HasValue) lead.FollowUpDate = data.FollowUpDate;
        if (data.LostReason is not null) lead.LostReason = data.LostReason.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetByIdAsync(lead.Id, cancellationToken))!;
    }
}
