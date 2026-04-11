using FluentAssertions;
using Moq;

using QBEngineer.Api.Features.ComplianceForms;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Handlers.ComplianceForms;

public class CreateSubmissionHandlerTests
{
    private readonly Mock<IDocumentSigningService> _signingService = new();

    [Fact]
    public async Task Handle_ValidCommand_CreatesSubmissionWithPendingStatus()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var template = new ComplianceFormTemplate
        {
            Name = "W-4",
            FormType = ComplianceFormType.W4,
            Description = "Federal W-4",
            Icon = "icon",
            IsActive = true,
            SortOrder = 1,
            ProfileCompletionKey = "w4",
            DocuSealTemplateId = 100,
            FormDefinitionVersions = [],
        };
        db.ComplianceFormTemplates.Add(template);
        await db.SaveChangesAsync();

        _signingService
            .Setup(s => s.CreateSubmissionAsync(100, "test@example.com", "John Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentSigningSubmission(999, "https://docuseal.example.com/submit/abc"));

        var handler = new CreateSubmissionHandler(db, _signingService.Object);
        var command = new CreateSubmissionCommand(42, template.Id, "test@example.com", "John Doe");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ComplianceSubmissionStatus.Pending);
        result.TemplateId.Should().Be(template.Id);
        result.DocuSealSubmitUrl.Should().Be("https://docuseal.example.com/submit/abc");

        db.ComplianceFormSubmissions.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var handler = new CreateSubmissionHandler(db, _signingService.Object);
        var command = new CreateSubmissionCommand(1, 999, "test@example.com", "John Doe");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_TemplateWithoutDocuSealId_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var template = new ComplianceFormTemplate
        {
            Name = "W-4",
            FormType = ComplianceFormType.W4,
            Description = "Federal W-4",
            Icon = "icon",
            IsActive = true,
            SortOrder = 1,
            ProfileCompletionKey = "w4",
            DocuSealTemplateId = null,
            FormDefinitionVersions = [],
        };
        db.ComplianceFormTemplates.Add(template);
        await db.SaveChangesAsync();

        var handler = new CreateSubmissionHandler(db, _signingService.Object);
        var command = new CreateSubmissionCommand(1, template.Id, "test@example.com", "John Doe");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no DocuSeal template*");
    }
}
