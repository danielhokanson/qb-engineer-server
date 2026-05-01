using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using QBEngineer.Api.Jobs;
using QBEngineer.Api.Services;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Integrations;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Jobs;

/// <summary>
/// Pillar 3 — proves the ReorderAnalysisJob migration didn't regress
/// back-compat behavior when no VendorPart is configured AND that the
/// new path is taken when a preferred VendorPart override exists.
/// </summary>
public class ReorderAnalysisJobMigrationTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 4, 30, 12, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock : Core.Interfaces.IClock
    {
        public DateTimeOffset UtcNow => FixedNow;
    }

    private static Part NewPart(
        string partNumber,
        int? leadTimeDays,
        int? safetyStockDays = null,
        decimal? minStockThreshold = null) => new()
        {
            PartNumber = partNumber,
            Name = partNumber,
            PartType = PartType.Part,
            Status = PartStatus.Active,
            LeadTimeDays = leadTimeDays,
            SafetyStockDays = safetyStockDays,
            MinStockThreshold = minStockThreshold,
        };

    private static async Task SeedConsumption(
        Data.Context.AppDbContext db, int partId, decimal qtyPerDay, int days)
    {
        // Seed BinMovements within 90-day window
        for (var d = 1; d <= days; d++)
        {
            db.BinMovements.Add(new BinMovement
            {
                EntityType = "part",
                EntityId = partId,
                Quantity = qtyPerDay,
                Reason = BinMovementReason.Pick,
                MovedAt = FixedNow.AddDays(-d),
            });
        }
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task RunAnalysis_NoVendorPart_UsesPartSnapshotLeadTime()
    {
        // Arrange — Part snapshot LeadTime=30 with a high burn rate that
        // falls below the cover threshold of (30 + 7) * burnRate.
        using var db = TestDbContextFactory.Create();
        var part = NewPart("REORDER-SNAP", leadTimeDays: 30, safetyStockDays: 7);
        db.Parts.Add(part);
        await db.SaveChangesAsync();

        // Burn rate ~5/day across 90 days (deterministic).
        await SeedConsumption(db, part.Id, qtyPerDay: 5m, days: 90);

        var job = new ReorderAnalysisJob(
            db, new FixedClock(), new PartSourcingResolver(db),
            NullLogger<ReorderAnalysisJob>.Instance);

        // Act
        await job.RunAnalysisAsync();

        // Assert — the cover-days uses 30 (snapshot), so a suggestion is
        // created. We just verify it exists for back-compat.
        db.ReorderSuggestions.Should().Contain(s => s.PartId == part.Id);
    }

    [Fact]
    public async Task RunAnalysis_PreferredVendorPartLeadTime_TakesPrecedence()
    {
        // Arrange — Part snapshot says leadTime=30 (would trigger reorder).
        // VendorPart override says leadTime=1 (would NOT trigger reorder
        // for the same stock + burn rate). Verify the override is honored.
        using var db = TestDbContextFactory.Create();
        var vendor = new Vendor { CompanyName = "Fast Vendor" };
        db.Vendors.Add(vendor);
        var part = NewPart("REORDER-OVR", leadTimeDays: 30, safetyStockDays: 7);
        db.Parts.Add(part);
        await db.SaveChangesAsync();

        // Seed a VendorPart that overrides the lead time DOWN to 1 day.
        db.VendorParts.Add(new VendorPart
        {
            VendorId = vendor.Id,
            PartId = part.Id,
            IsPreferred = true,
            LeadTimeDays = 1,
        });
        await db.SaveChangesAsync();

        // Burn rate ~5/day for 90 days.
        await SeedConsumption(db, part.Id, qtyPerDay: 5m, days: 90);

        // Seed a stock that's enough for cover at leadTime=1 (1+7)*5=40
        // but would be insufficient for leadTime=30 ((30+7)*5=185).
        db.BinContents.Add(new BinContent
        {
            EntityType = "part",
            EntityId = part.Id,
            Quantity = 100m,
            ReservedQuantity = 0m,
            LocationId = 1,
        });
        await db.SaveChangesAsync();

        var job = new ReorderAnalysisJob(
            db, new FixedClock(), new PartSourcingResolver(db),
            NullLogger<ReorderAnalysisJob>.Instance);

        // Act
        await job.RunAnalysisAsync();

        // Assert — VendorPart override (leadTime=1) means stock is
        // sufficient for cover, so NO suggestion is created. With the
        // pre-migration snapshot read (leadTime=30), a suggestion would
        // have been created. This proves the resolver is being used.
        db.ReorderSuggestions.Should().NotContain(s => s.PartId == part.Id);
    }
}
