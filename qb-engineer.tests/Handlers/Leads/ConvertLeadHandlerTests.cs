using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

using QBEngineer.Api.Features.Leads;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Handlers.Leads;

public class ConvertLeadHandlerTests
{
    private readonly AppDbContext _db = TestDbContextFactory.Create();
    private readonly Mock<ILeadRepository> _leadRepo = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly ConvertLeadHandler _handler;

    public ConvertLeadHandlerTests()
    {
        _db.CurrentUserId = 1;
        _handler = new ConvertLeadHandler(_leadRepo.Object, _db, _mediator.Object);
    }

    private async Task<Lead> SeedLead(string companyName = "Acme Co", string? contactName = "Jane Smith",
        string? source = "Website", LeadStatus status = LeadStatus.New)
    {
        var lead = new Lead
        {
            CompanyName = companyName,
            ContactName = contactName,
            Email = "lead@acme.test",
            Phone = "555-0100",
            Source = source,
            Notes = "Initial inquiry",
            Status = status,
            CreatedBy = 1,
        };
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync();

        _leadRepo.Setup(r => r.FindAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        return lead;
    }

    [Fact]
    public async Task Handle_BasicConversion_CreatesCustomerAndContact()
    {
        var lead = await SeedLead();

        var result = await _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        var customer = await _db.Customers.FirstAsync(c => c.Id == result.CustomerId);
        customer.CompanyName.Should().Be("Acme Co");
        customer.Email.Should().Be("lead@acme.test");

        var contact = await _db.Contacts.FirstAsync(c => c.CustomerId == customer.Id);
        contact.FirstName.Should().Be("Jane");
        contact.LastName.Should().Be("Smith");
        contact.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LeadStatusBecomesConvertedWithBackLink()
    {
        var lead = await SeedLead();

        var result = await _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        lead.Status.Should().Be(LeadStatus.Converted);
        lead.ConvertedCustomerId.Should().Be(result.CustomerId);
    }

    [Fact]
    public async Task Handle_LogsActivityOnBothLeadAndCustomerAnchors()
    {
        var lead = await SeedLead();

        var result = await _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        // Indexing-points rule — the conversion event is the canonical
        // Lead↔Customer bridge, so it must surface on BOTH activity tabs.
        var leadRows = await _db.ActivityLogs
            .Where(a => a.EntityType == "Lead" && a.EntityId == lead.Id && a.Action == "lead-converted")
            .ToListAsync();
        leadRows.Should().HaveCount(1, "the lead's activity tab should show the conversion event");

        var customerRows = await _db.ActivityLogs
            .Where(a => a.EntityType == "Customer" && a.EntityId == result.CustomerId && a.Action == "lead-converted")
            .ToListAsync();
        customerRows.Should().HaveCount(1, "the customer's activity tab should show the conversion provenance");

        leadRows[0].Description.Should().Contain("Acme Co");
        leadRows[0].Description.Should().Contain("Website");
        customerRows[0].Description.Should().Contain("Acme Co");
    }

    [Fact]
    public async Task Handle_AlreadyConvertedLead_Throws()
    {
        var lead = await SeedLead(status: LeadStatus.Converted);

        var act = () => _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already*");
    }

    [Fact]
    public async Task Handle_LostLead_Throws()
    {
        var lead = await SeedLead(status: LeadStatus.Lost);

        var act = () => _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*lost*");
    }

    [Fact]
    public async Task Handle_NoContactName_SkipsContactCreation()
    {
        var lead = await SeedLead(contactName: null);

        var result = await _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        var contacts = await _db.Contacts.Where(c => c.CustomerId == result.CustomerId).ToListAsync();
        contacts.Should().BeEmpty();
    }

    // Defensive-name-parse coverage. The previous implementation used
    // Split(' ', 2) which silently mangled comma-form ("Smith, John"),
    // middle names ("John A Smith"), and various edge cases. These specs
    // pin the new ParseContactName behavior so a future refactor doesn't
    // regress to the old form.

    [Theory]
    [InlineData("Jane Smith", "Jane", "Smith")]
    [InlineData("John Allen Smith", "John Allen", "Smith")]
    [InlineData("Mary O'Brien", "Mary", "O'Brien")]
    [InlineData("Smith, John", "John", "Smith")]
    [InlineData("Smith, John M", "John M", "Smith")]
    [InlineData("  Smith,   John  ", "John", "Smith")]
    [InlineData("Madonna", "Madonna", "")]
    [InlineData("  ", "", "")]
    [InlineData("", "", "")]
    public async Task Handle_NameParsing_HandlesCommonForms(string input, string expectedFirst, string expectedLast)
    {
        // Empty / whitespace input → no contact created at all (handler
        // short-circuits on string.IsNullOrWhiteSpace before calling
        // ParseContactName), so test that path separately.
        if (string.IsNullOrWhiteSpace(input))
        {
            var emptyLead = await SeedLead(contactName: input);
            var emptyResult = await _handler.Handle(new ConvertLeadCommand(emptyLead.Id, false), CancellationToken.None);
            var emptyContacts = await _db.Contacts.Where(c => c.CustomerId == emptyResult.CustomerId).ToListAsync();
            emptyContacts.Should().BeEmpty();
            return;
        }

        var lead = await SeedLead(contactName: input);

        var result = await _handler.Handle(new ConvertLeadCommand(lead.Id, false), CancellationToken.None);

        var contact = await _db.Contacts.FirstAsync(c => c.CustomerId == result.CustomerId);
        contact.FirstName.Should().Be(expectedFirst);
        contact.LastName.Should().Be(expectedLast);
    }
}
