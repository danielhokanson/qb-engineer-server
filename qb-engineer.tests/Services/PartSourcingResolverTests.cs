using FluentAssertions;

using QBEngineer.Api.Services;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Services;

/// <summary>
/// Pillar 3 — coverage for the IPartSourcingResolver implementation.
/// Verifies the per-column coalescing rules: prefer VendorPart when
/// configured, fall back to Part snapshot for any column the preferred
/// VendorPart leaves null.
/// </summary>
public class PartSourcingResolverTests
{
    private static Part NewPart(
        string partNumber,
        int? leadTimeDays = null,
        int? minOrderQty = null,
        int? packSize = null) => new()
        {
            PartNumber = partNumber,
            Name = partNumber,
            ProcurementSource = ProcurementSource.Buy,
            InventoryClass = InventoryClass.Component,
            Status = PartStatus.Active,
            LeadTimeDays = leadTimeDays,
            MinOrderQty = minOrderQty,
            PackSize = packSize,
        };

    [Fact]
    public async Task ResolveAsync_NoVendorPart_FallsBackToPartSnapshot()
    {
        // Arrange — Part snapshot only
        using var db = TestDbContextFactory.Create();
        var part = NewPart("SNAP-001", leadTimeDays: 21, minOrderQty: 50, packSize: 10);
        db.Parts.Add(part);
        await db.SaveChangesAsync();

        var resolver = new PartSourcingResolver(db);

        // Act
        var result = await resolver.ResolveAsync(part.Id, CancellationToken.None);

        // Assert
        result.PartId.Should().Be(part.Id);
        result.PreferredVendorId.Should().BeNull();
        result.LeadTimeDays.Should().Be(21);
        result.MinOrderQty.Should().Be(50m);
        result.PackSize.Should().Be(10m);
        result.ResolvedFromVendorPart.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveAsync_PreferredVendorPart_OverridesPartSnapshot()
    {
        // Arrange — preferred VendorPart with non-null overrides on every column
        using var db = TestDbContextFactory.Create();
        var vendor = new Vendor { CompanyName = "Acme Supply" };
        db.Vendors.Add(vendor);
        var part = NewPart("OVR-001", leadTimeDays: 21, minOrderQty: 50, packSize: 10);
        db.Parts.Add(part);
        await db.SaveChangesAsync();

        db.VendorParts.Add(new VendorPart
        {
            VendorId = vendor.Id,
            PartId = part.Id,
            IsPreferred = true,
            LeadTimeDays = 7,
            MinOrderQty = 100m,
            PackSize = 25m,
        });
        await db.SaveChangesAsync();

        var resolver = new PartSourcingResolver(db);

        // Act
        var result = await resolver.ResolveAsync(part.Id, CancellationToken.None);

        // Assert
        result.PreferredVendorId.Should().Be(vendor.Id);
        result.LeadTimeDays.Should().Be(7);
        result.MinOrderQty.Should().Be(100m);
        result.PackSize.Should().Be(25m);
        result.ResolvedFromVendorPart.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_PreferredVendorPartWithNulls_CoalescesPerColumn()
    {
        // Arrange — preferred VendorPart with NULL overrides; the per-column
        // coalesce should fall back to the Part snapshot for each null column.
        using var db = TestDbContextFactory.Create();
        var vendor = new Vendor { CompanyName = "Partial Supply" };
        db.Vendors.Add(vendor);
        var part = NewPart("MIX-001", leadTimeDays: 14, minOrderQty: 25, packSize: 5);
        db.Parts.Add(part);
        await db.SaveChangesAsync();

        db.VendorParts.Add(new VendorPart
        {
            VendorId = vendor.Id,
            PartId = part.Id,
            IsPreferred = true,
            LeadTimeDays = 3,        // override
            MinOrderQty = null,      // fall back to snapshot
            PackSize = null,         // fall back to snapshot
        });
        await db.SaveChangesAsync();

        var resolver = new PartSourcingResolver(db);

        // Act
        var result = await resolver.ResolveAsync(part.Id, CancellationToken.None);

        // Assert
        result.PreferredVendorId.Should().Be(vendor.Id);
        result.LeadTimeDays.Should().Be(3);          // from VendorPart override
        result.MinOrderQty.Should().Be(25m);         // fell back to Part
        result.PackSize.Should().Be(5m);             // fell back to Part
        result.ResolvedFromVendorPart.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_NonPreferredVendorPart_IsIgnored()
    {
        // Arrange — there's a VendorPart row but it's NOT preferred. Should
        // behave as if there was no VendorPart at all.
        using var db = TestDbContextFactory.Create();
        var vendor = new Vendor { CompanyName = "Alt Vendor" };
        db.Vendors.Add(vendor);
        var part = NewPart("NONPREF-001", leadTimeDays: 14, minOrderQty: 25, packSize: 5);
        db.Parts.Add(part);
        await db.SaveChangesAsync();

        db.VendorParts.Add(new VendorPart
        {
            VendorId = vendor.Id,
            PartId = part.Id,
            IsPreferred = false,
            LeadTimeDays = 3,
            MinOrderQty = 999m,
            PackSize = 999m,
        });
        await db.SaveChangesAsync();

        var resolver = new PartSourcingResolver(db);

        // Act
        var result = await resolver.ResolveAsync(part.Id, CancellationToken.None);

        // Assert — non-preferred row is ignored, snapshot wins.
        result.PreferredVendorId.Should().BeNull();
        result.LeadTimeDays.Should().Be(14);
        result.MinOrderQty.Should().Be(25m);
        result.PackSize.Should().Be(5m);
        result.ResolvedFromVendorPart.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveManyAsync_MixedParts_ResolvesEachIndependently()
    {
        // Arrange — three parts: one snapshot-only, one fully overridden, one mixed.
        using var db = TestDbContextFactory.Create();
        var vendor = new Vendor { CompanyName = "Multi" };
        db.Vendors.Add(vendor);
        var snapOnly = NewPart("BULK-SNAP", leadTimeDays: 30, minOrderQty: 10, packSize: 2);
        var fullOver = NewPart("BULK-OVR", leadTimeDays: 30, minOrderQty: 10, packSize: 2);
        var mixed = NewPart("BULK-MIX", leadTimeDays: 30, minOrderQty: 10, packSize: 2);
        db.Parts.AddRange(snapOnly, fullOver, mixed);
        await db.SaveChangesAsync();

        db.VendorParts.AddRange(
            new VendorPart
            {
                VendorId = vendor.Id, PartId = fullOver.Id, IsPreferred = true,
                LeadTimeDays = 4, MinOrderQty = 99m, PackSize = 11m,
            },
            new VendorPart
            {
                VendorId = vendor.Id, PartId = mixed.Id, IsPreferred = true,
                LeadTimeDays = null, MinOrderQty = 7m, PackSize = null,
            });
        await db.SaveChangesAsync();

        var resolver = new PartSourcingResolver(db);

        // Act
        var result = await resolver.ResolveManyAsync(
            new[] { snapOnly.Id, fullOver.Id, mixed.Id },
            CancellationToken.None);

        // Assert
        result.Should().ContainKey(snapOnly.Id).WhoseValue.LeadTimeDays.Should().Be(30);
        result[snapOnly.Id].ResolvedFromVendorPart.Should().BeFalse();
        result[snapOnly.Id].PackSize.Should().Be(2m);

        result[fullOver.Id].LeadTimeDays.Should().Be(4);
        result[fullOver.Id].MinOrderQty.Should().Be(99m);
        result[fullOver.Id].PackSize.Should().Be(11m);
        result[fullOver.Id].ResolvedFromVendorPart.Should().BeTrue();

        result[mixed.Id].LeadTimeDays.Should().Be(30);   // null on VP -> snapshot
        result[mixed.Id].MinOrderQty.Should().Be(7m);    // override
        result[mixed.Id].PackSize.Should().Be(2m);       // null on VP -> snapshot
        result[mixed.Id].ResolvedFromVendorPart.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveManyAsync_EmptyInput_ReturnsEmptyDictionary()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var resolver = new PartSourcingResolver(db);

        // Act
        var result = await resolver.ResolveManyAsync(Array.Empty<int>(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
