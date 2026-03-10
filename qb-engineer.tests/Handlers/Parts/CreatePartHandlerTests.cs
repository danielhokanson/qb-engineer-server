using Bogus;
using FluentAssertions;
using Moq;
using QBEngineer.Api.Features.Parts;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Tests.Handlers.Parts;

public class CreatePartHandlerTests
{
    private readonly Mock<IPartRepository> _partRepo = new();
    private readonly CreatePartHandler _handler;

    private readonly Faker _faker = new();

    public CreatePartHandlerTests()
    {
        _handler = new CreatePartHandler(_partRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesPartWithUpperCaseNumber()
    {
        // Arrange
        var partNumber = _faker.Random.AlphaNumeric(8).ToLower();
        var description = _faker.Commerce.ProductName();
        var material = _faker.Commerce.ProductMaterial();

        _partRepo.Setup(r => r.PartNumberExistsAsync(partNumber, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedResult = new PartDetailResponseModel(
            1, partNumber.Trim().ToUpper(), description, "A", PartStatus.Draft, PartType.Part,
            material, null, null, null, null, null, null, null, null,
            new List<BOMEntryResponseModel>(), new List<BOMUsageResponseModel>(),
            DateTime.UtcNow, DateTime.UtcNow);

        _partRepo.Setup(r => r.GetDetailAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new CreatePartCommand(partNumber, description, null, PartType.Part, material, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        _partRepo.Verify(r => r.AddAsync(It.Is<Part>(p =>
            p.PartNumber == partNumber.Trim().ToUpper() &&
            p.Description == description.Trim() &&
            p.Revision == "A" &&
            p.Status == PartStatus.Draft &&
            p.PartType == PartType.Part &&
            p.Material == material.Trim()
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicatePartNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var partNumber = "PART-001";

        _partRepo.Setup(r => r.PartNumberExistsAsync(partNumber, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreatePartCommand(partNumber, "Description", null, PartType.Part, null, null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*'{partNumber}'*already exists*");
    }

    [Fact]
    public async Task Handle_NoRevision_DefaultsToA()
    {
        // Arrange
        _partRepo.Setup(r => r.PartNumberExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedResult = new PartDetailResponseModel(
            1, "TEST-001", "Test Part", "A", PartStatus.Draft, PartType.Part,
            null, null, null, null, null, null, null, null, null,
            new List<BOMEntryResponseModel>(), new List<BOMUsageResponseModel>(),
            DateTime.UtcNow, DateTime.UtcNow);

        _partRepo.Setup(r => r.GetDetailAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new CreatePartCommand("test-001", "Test Part", null, PartType.Part, null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _partRepo.Verify(r => r.AddAsync(It.Is<Part>(p =>
            p.Revision == "A"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRevision_UsesProvidedRevision()
    {
        // Arrange
        _partRepo.Setup(r => r.PartNumberExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedResult = new PartDetailResponseModel(
            1, "TEST-001", "Test Part", "C", PartStatus.Draft, PartType.Assembly,
            null, null, null, null, null, null, null, null, null,
            new List<BOMEntryResponseModel>(), new List<BOMUsageResponseModel>(),
            DateTime.UtcNow, DateTime.UtcNow);

        _partRepo.Setup(r => r.GetDetailAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new CreatePartCommand("test-001", "Test Part", "C", PartType.Assembly, null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _partRepo.Verify(r => r.AddAsync(It.Is<Part>(p =>
            p.Revision == "C" &&
            p.PartType == PartType.Assembly
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StatusAlwaysDraft()
    {
        // Arrange
        _partRepo.Setup(r => r.PartNumberExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedResult = new PartDetailResponseModel(
            1, "TEST-001", "Test", "A", PartStatus.Draft, PartType.Assembly,
            null, null, null, null, null, null, null, null, null,
            new List<BOMEntryResponseModel>(), new List<BOMUsageResponseModel>(),
            DateTime.UtcNow, DateTime.UtcNow);

        _partRepo.Setup(r => r.GetDetailAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new CreatePartCommand("test-001", "Test", null, PartType.Assembly, null, null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _partRepo.Verify(r => r.AddAsync(It.Is<Part>(p =>
            p.Status == PartStatus.Draft
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TrimsWhitespace()
    {
        // Arrange
        _partRepo.Setup(r => r.PartNumberExistsAsync("  part-001  ", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedResult = new PartDetailResponseModel(
            1, "PART-001", "Trimmed", "B", PartStatus.Draft, PartType.Part,
            "Steel", "M-100", null, null, null, null, null, null, null,
            new List<BOMEntryResponseModel>(), new List<BOMUsageResponseModel>(),
            DateTime.UtcNow, DateTime.UtcNow);

        _partRepo.Setup(r => r.GetDetailAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var command = new CreatePartCommand("  part-001  ", "  Trimmed  ", "  B  ", PartType.Part, "  Steel  ", "  M-100  ");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _partRepo.Verify(r => r.AddAsync(It.Is<Part>(p =>
            p.PartNumber == "PART-001" &&
            p.Description == "Trimmed" &&
            p.Revision == "B" &&
            p.Material == "Steel" &&
            p.MoldToolRef == "M-100"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
