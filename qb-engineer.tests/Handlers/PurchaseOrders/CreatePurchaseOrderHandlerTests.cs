using Bogus;
using FluentAssertions;
using Moq;
using QBEngineer.Api.Features.PurchaseOrders;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Tests.Handlers.PurchaseOrders;

public class CreatePurchaseOrderHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _poRepo = new();
    private readonly Mock<IVendorRepository> _vendorRepo = new();
    private readonly Mock<IPartRepository> _partRepo = new();
    private readonly CreatePurchaseOrderHandler _handler;

    private readonly Faker _faker = new();

    public CreatePurchaseOrderHandlerTests()
    {
        _handler = new CreatePurchaseOrderHandler(_poRepo.Object, _vendorRepo.Object, _partRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesPOAndReturnsListItem()
    {
        // Arrange
        var vendorId = _faker.Random.Int(1, 100);
        var partId = _faker.Random.Int(1, 100);
        var poNumber = $"PO-{_faker.Random.Int(1000, 9999)}";
        var vendor = new Vendor { Id = vendorId, CompanyName = _faker.Company.CompanyName() };
        var part = new Part { Id = partId, PartNumber = "P-001", Description = "Test Part" };

        _vendorRepo.Setup(r => r.FindAsync(vendorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendor);
        _poRepo.Setup(r => r.GenerateNextPONumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(poNumber);
        _partRepo.Setup(r => r.FindAsync(partId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);

        var command = new CreatePurchaseOrderCommand(
            vendorId, null, "Test notes",
            [new CreatePurchaseOrderLineModel(partId, null, 10, 5.50m, null)]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PONumber.Should().Be(poNumber);
        result.VendorId.Should().Be(vendorId);
        result.VendorName.Should().Be(vendor.CompanyName);
        result.LineCount.Should().Be(1);
        result.TotalOrdered.Should().Be(10);

        _poRepo.Verify(r => r.AddAsync(It.Is<PurchaseOrder>(po =>
            po.PONumber == poNumber &&
            po.VendorId == vendorId &&
            po.Notes == "Test notes" &&
            po.Lines.Count == 1
        ), It.IsAny<CancellationToken>()), Times.Once);

        _poRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_VendorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _vendorRepo.Setup(r => r.FindAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vendor?)null);

        var command = new CreatePurchaseOrderCommand(
            999, null, null,
            [new CreatePurchaseOrderLineModel(1, null, 5, 1.00m, null)]);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Vendor 999*");
    }

    [Fact]
    public async Task Handle_PartNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var vendorId = 1;
        var vendor = new Vendor { Id = vendorId, CompanyName = "Test Vendor" };

        _vendorRepo.Setup(r => r.FindAsync(vendorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendor);
        _poRepo.Setup(r => r.GenerateNextPONumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("PO-0001");
        _partRepo.Setup(r => r.FindAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Part?)null);

        var command = new CreatePurchaseOrderCommand(
            vendorId, null, null,
            [new CreatePurchaseOrderLineModel(999, null, 5, 1.00m, null)]);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Part 999*");
    }

    [Fact]
    public async Task Handle_LineWithoutDescription_UsesPartDescription()
    {
        // Arrange
        var vendorId = 1;
        var partId = 2;
        var vendor = new Vendor { Id = vendorId, CompanyName = "Vendor" };
        var part = new Part { Id = partId, PartNumber = "P-001", Description = "Default Part Description" };

        _vendorRepo.Setup(r => r.FindAsync(vendorId, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _poRepo.Setup(r => r.GenerateNextPONumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("PO-0001");
        _partRepo.Setup(r => r.FindAsync(partId, It.IsAny<CancellationToken>())).ReturnsAsync(part);

        var command = new CreatePurchaseOrderCommand(
            vendorId, null, null,
            [new CreatePurchaseOrderLineModel(partId, null, 1, 10m, null)]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _poRepo.Verify(r => r.AddAsync(It.Is<PurchaseOrder>(po =>
            po.Lines.First().Description == "Default Part Description"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LineWithCustomDescription_UsesProvidedDescription()
    {
        // Arrange
        var vendorId = 1;
        var partId = 2;
        var vendor = new Vendor { Id = vendorId, CompanyName = "Vendor" };
        var part = new Part { Id = partId, PartNumber = "P-001", Description = "Part Desc" };

        _vendorRepo.Setup(r => r.FindAsync(vendorId, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _poRepo.Setup(r => r.GenerateNextPONumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("PO-0001");
        _partRepo.Setup(r => r.FindAsync(partId, It.IsAny<CancellationToken>())).ReturnsAsync(part);

        var command = new CreatePurchaseOrderCommand(
            vendorId, null, null,
            [new CreatePurchaseOrderLineModel(partId, "Custom Description", 1, 10m, null)]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _poRepo.Verify(r => r.AddAsync(It.Is<PurchaseOrder>(po =>
            po.Lines.First().Description == "Custom Description"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleLines_CreatesAllLines()
    {
        // Arrange
        var vendorId = 1;
        var vendor = new Vendor { Id = vendorId, CompanyName = "Vendor" };
        var part1 = new Part { Id = 1, PartNumber = "P-001", Description = "Part 1" };
        var part2 = new Part { Id = 2, PartNumber = "P-002", Description = "Part 2" };

        _vendorRepo.Setup(r => r.FindAsync(vendorId, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _poRepo.Setup(r => r.GenerateNextPONumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("PO-0001");
        _partRepo.Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(part1);
        _partRepo.Setup(r => r.FindAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(part2);

        var command = new CreatePurchaseOrderCommand(
            vendorId, null, null,
            [
                new CreatePurchaseOrderLineModel(1, null, 5, 10m, null),
                new CreatePurchaseOrderLineModel(2, null, 3, 20m, null),
            ]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LineCount.Should().Be(2);
        result.TotalOrdered.Should().Be(8);
    }
}
