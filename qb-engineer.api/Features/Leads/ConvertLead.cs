using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Features.Jobs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;
using QBEngineer.Data.Extensions;

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

        // Create customer from lead. Provenance is captured via the
        // bidirectional Lead.ConvertedCustomerId / Customer.SourceLead pair
        // wired below — no second FK column needed on Customer.
        var customer = new Customer
        {
            Name = lead.CompanyName,
            CompanyName = lead.CompanyName,
            Email = lead.Email,
            Phone = lead.Phone,
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        // Create contact if contact name is available. Defensive name parse:
        // handles "First Last", "First Middle Last" (last word = last name),
        // "Last, First" / "Last, First MI" (comma convention), and single
        // names. The previous Split(' ', 2) silently broke any name with
        // a middle, suffix, or comma form.
        Contact? primaryContact = null;
        if (!string.IsNullOrWhiteSpace(lead.ContactName))
        {
            var (firstName, lastName) = ParseContactName(lead.ContactName);
            primaryContact = new Contact
            {
                CustomerId = customer.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = lead.Email,
                Phone = lead.Phone,
                IsPrimary = true,
            };
            db.Contacts.Add(primaryContact);
        }

        // Update lead — close it out, link to the new customer.
        lead.Status = LeadStatus.Converted;
        lead.ConvertedCustomerId = customer.Id;

        // Indexing-points rule — the conversion is the canonical Lead↔Customer
        // bridge moment, so the activity row appears on BOTH anchors. The
        // lead's activity tab will show "converted to customer #N" and the
        // customer's will show "converted from lead #N — companyName".
        var conversionDescription =
            $"Converted lead → customer: {lead.CompanyName}" +
            (string.IsNullOrEmpty(lead.Source) ? "" : $" (source: {lead.Source})") +
            (string.IsNullOrEmpty(lead.ContactName) ? "" : $" — contact: {lead.ContactName}");
        db.LogActivityAt(
            "lead-converted",
            conversionDescription,
            ("Lead", lead.Id),
            ("Customer", customer.Id));

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

    /// <summary>
    /// Parse a free-form contact name into (firstName, lastName). Defensive
    /// against the common conventions: "First Last", "First Middle Last",
    /// "Last, First", "Last, First MI", and single-word names. Empty input
    /// returns ("", ""). Whitespace-only segments are normalised away.
    /// </summary>
    private static (string firstName, string lastName) ParseContactName(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.Length == 0) return ("", "");

        // Comma form: "Last, First [MI]" — common in formal contexts. Take
        // the part before the comma as last, the rest as first.
        var commaIdx = trimmed.IndexOf(',');
        if (commaIdx > 0)
        {
            var lastPart = trimmed[..commaIdx].Trim();
            var firstPart = trimmed[(commaIdx + 1)..].Trim();
            return (firstPart, lastPart);
        }

        // Space form: last word is last name, everything before is first
        // name (covering "First Last", "First Middle Last", honorifics).
        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return (parts[0], "");
        return (string.Join(' ', parts[..^1]), parts[^1]);
    }
}
