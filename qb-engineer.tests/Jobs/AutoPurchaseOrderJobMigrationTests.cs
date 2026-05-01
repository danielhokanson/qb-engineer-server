using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using QBEngineer.Api.Features.AutoPo;
using QBEngineer.Api.Jobs;
using QBEngineer.Api.Services;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;
using QBEngineer.Integrations;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Jobs;

/// <summary>
/// Pillar 3 — proves the AutoPurchaseOrderJob migration honors per-vendor
/// PackSize / MinOrderQty overrides via the IPartSourcingResolver while
/// staying back-compat when no preferred VendorPart row is configured.
///
/// We exercise the Suggest mode path so the test does not need to wire
/// up <see cref="PurchaseOrderGenerator"/> + IPurchaseOrderRepository +
/// IBarcodeService. Suggest mode writes <see cref="AutoPoSuggestion"/>
/// rows whose <c>SuggestedQty</c> reflects the resolved sourcing values.
/// </summary>
public class AutoPurchaseOrderJobMigrationTests
{
    private static async Task<(int VendorId, int ParentId, int ChildId)> SeedScenarioAsync(
        AppDbContext db,
        decimal soQty,
        decimal bomQuantityPerUnit,
        int? snapshotPackSize,
        int? snapshotMinOrderQty,
        decimal? vendorPartPackSize = null,
        decimal? vendorPartMinOrderQty = null,
        bool createPreferredVendorPart = false)
    {
        var vendor = new Vendor
        {
            CompanyName = "AutoPO Vendor",
            AutoPoMode = AutoPoMode.Suggest,
        };
        db.Vendors.Add(vendor);

        var parent = new Part
        {
            PartNumber = "PARENT-001", Name = "Parent",
            PartType = PartType.Part, Status = PartStatus.Active,
        };
        var child = new Part
        {
            PartNumber = "CHILD-001", Name = "Child",
            PartType = PartType.Part, Status = PartStatus.Active,
            PreferredVendorId = null,
            PackSize = snapshotPackSize,
            MinOrderQty = snapshotMinOrderQty,
        };
        db.Parts.AddRange(parent, child);
        await db.SaveChangesAsync();

        // Wire the child's preferred vendor to our test vendor (used by
        // the job to pick the vendor for the suggestion).
        child.PreferredVendorId = vendor.Id;
        await db.SaveChangesAsync();

        if (createPreferredVendorPart)
        {
            db.VendorParts.Add(new VendorPart
            {
                VendorId = vendor.Id,
                PartId = child.Id,
                IsPreferred = true,
                PackSize = vendorPartPackSize,
                MinOrderQty = vendorPartMinOrderQty,
            });
            await db.SaveChangesAsync();
        }

        // BOM: parent consumes child at the configured ratio.
        db.BOMEntries.Add(new BOMEntry
        {
            ParentPartId = parent.Id,
            ChildPartId = child.Id,
            Quantity = bomQuantityPerUnit,
            SourceType = BOMSourceType.Buy,
        });

        // Customer + open SO + line so demand is non-zero.
        var customer = new Customer { Name = "Test Customer" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var so = new SalesOrder
        {
            OrderNumber = "SO-AUTO-001",
            CustomerId = customer.Id,
            Status = SalesOrderStatus.Confirmed,
            RequestedDeliveryDate = DateTimeOffset.UtcNow.AddDays(30),
        };
        db.SalesOrders.Add(so);
        await db.SaveChangesAsync();

        db.SalesOrderLines.Add(new SalesOrderLine
        {
            SalesOrderId = so.Id,
            PartId = parent.Id,
            Description = "Parent",
            Quantity = soQty,
            UnitPrice = 1m,
        });

        // Auto-PO master switch ON, mode Suggest.
        db.SystemSettings.AddRange(
            new SystemSetting { Key = "inventory:auto_po_enabled", Value = "true" },
            new SystemSetting { Key = "inventory:auto_po_mode", Value = "Suggest" });

        await db.SaveChangesAsync();
        return (vendor.Id, parent.Id, child.Id);
    }

    private static AutoPurchaseOrderJob BuildJob(AppDbContext db)
    {
        var settingsRepo = new Mock<ISystemSettingRepository>();
        settingsRepo.Setup(r => r.FindByKeyAsync("inventory:auto_po_enabled", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemSetting { Key = "inventory:auto_po_enabled", Value = "true" });
        settingsRepo.Setup(r => r.FindByKeyAsync("inventory:auto_po_mode", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemSetting { Key = "inventory:auto_po_mode", Value = "Suggest" });
        settingsRepo.Setup(r => r.FindByKeyAsync("inventory:auto_po_buffer_days", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemSetting { Key = "inventory:auto_po_buffer_days", Value = "3" });

        // PurchaseOrderGenerator is concrete — but Suggest mode never calls
        // it. Constructing one with mock collaborators is acceptable.
        var poGen = new PurchaseOrderGenerator(
            db,
            Mock.Of<IPurchaseOrderRepository>(),
            Mock.Of<IBarcodeService>());

        return new AutoPurchaseOrderJob(
            db,
            new SystemClock(),
            settingsRepo.Object,
            poGen,
            new PartSourcingResolver(db),
            NullLogger<AutoPurchaseOrderJob>.Instance);
    }

    [Fact]
    public async Task Execute_NoVendorPart_RoundsToPartSnapshotPackSizeAndMinOrderQty()
    {
        // Arrange — snapshot only: PackSize=10, MinOrderQty=50. With SO of
        // 13 parents and 1 child per parent, raw shortfall = 13. After
        // rounding to PackSize 10 -> 20. Min check (50) bumps to 50.
        using var db = TestDbContextFactory.Create();
        var (_, _, childId) = await SeedScenarioAsync(
            db,
            soQty: 13m,
            bomQuantityPerUnit: 1m,
            snapshotPackSize: 10,
            snapshotMinOrderQty: 50,
            createPreferredVendorPart: false);

        var job = BuildJob(db);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert
        var suggestion = db.AutoPoSuggestions.SingleOrDefault(s => s.PartId == childId);
        suggestion.Should().NotBeNull();
        suggestion!.SuggestedQty.Should().Be(50m);
    }

    [Fact]
    public async Task Execute_PreferredVendorPart_OverridesSnapshotPackSizeAndMinOrderQty()
    {
        // Arrange — snapshot: PackSize=10 / MinOrderQty=50. VendorPart
        // override: PackSize=4 / MinOrderQty=8. SO 13 parents x 1 BOM qty
        // = shortfall 13 -> rounded up to next 4 -> 16. Min (8) is below
        // 16 so no bump. Pre-migration code would have used PackSize=10
        // (rounded 13 to 20) then min 50 -> 50.
        using var db = TestDbContextFactory.Create();
        var (_, _, childId) = await SeedScenarioAsync(
            db,
            soQty: 13m,
            bomQuantityPerUnit: 1m,
            snapshotPackSize: 10,
            snapshotMinOrderQty: 50,
            vendorPartPackSize: 4m,
            vendorPartMinOrderQty: 8m,
            createPreferredVendorPart: true);

        var job = BuildJob(db);

        // Act
        await job.Execute(CancellationToken.None);

        // Assert — VendorPart override wins.
        var suggestion = db.AutoPoSuggestions.SingleOrDefault(s => s.PartId == childId);
        suggestion.Should().NotBeNull();
        suggestion!.SuggestedQty.Should().Be(16m);
    }
}
