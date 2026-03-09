using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Features.Jobs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Leads;

public record ConvertLeadCommand(int LeadId, bool CreateJob) : IRequest<ConvertLeadResponseModel>;

public class ConvertLeadHandler(
    ILeadRepository leadRepo,
    AppDbContext db,
    IMediator mediator) : IRequestHandler<ConvertLeadCommand, ConvertLeadResponseModel>
{
    public async Task<ConvertLeadResponseModel> Handle(ConvertLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadRepo.FindAsync(request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead {request.LeadId} not found");

        if (lead.Status is LeadStatus.Converted)
            throw new InvalidOperationException("Lead has already been converted.");

        if (lead.Status is LeadStatus.Lost)
            throw new InvalidOperationException("Cannot convert a lost lead.");

        // Create customer from lead
        var customer = new Customer
        {
            Name = lead.CompanyName,
            CompanyName = lead.CompanyName,
            Email = lead.Email,
            Phone = lead.Phone,
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        // Create contact if contact name is available
        if (!string.IsNullOrWhiteSpace(lead.ContactName))
        {
            var names = lead.ContactName.Trim().Split(' ', 2);
            var contact = new Contact
            {
                CustomerId = customer.Id,
                FirstName = names[0],
                LastName = names.Length > 1 ? names[1] : string.Empty,
                Email = lead.Email,
                Phone = lead.Phone,
                IsPrimary = true,
            };
            db.Contacts.Add(contact);
        }

        // Update lead
        lead.Status = LeadStatus.Converted;
        lead.ConvertedCustomerId = customer.Id;
        await db.SaveChangesAsync(cancellationToken);

        // Optionally create a job
        int? jobId = null;
        if (request.CreateJob)
        {
            // Find default track type
            var defaultTrackType = await db.Set<TrackType>()
                .Where(t => t.IsDefault && t.IsActive)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (defaultTrackType > 0)
            {
                var jobResult = await mediator.Send(new CreateJobCommand(
                    Title: $"New Job — {lead.CompanyName}",
                    Description: lead.Notes,
                    TrackTypeId: defaultTrackType,
                    AssigneeId: null,
                    CustomerId: customer.Id,
                    Priority: null,
                    DueDate: null), cancellationToken);
                jobId = jobResult.Id;
            }
        }

        return new ConvertLeadResponseModel(customer.Id, jobId);
    }
}
