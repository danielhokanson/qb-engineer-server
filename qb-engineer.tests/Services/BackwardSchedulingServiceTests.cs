using FluentAssertions;

using QBEngineer.Api.Services;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Integrations;
using QBEngineer.Tests.Helpers;

namespace QBEngineer.Tests.Services;

/// <summary>
/// Polish-pass follow-up — covers the latent gap flagged by the reader
/// migration agent (b8cf818): when a Buy BOMEntry has a NULL per-line
/// LeadTimeDays, the service must fall back to <see cref="IPartSourcingResolver"/>
/// for the child part's effective lead time rather than silently dropping
/// the row.
/// </summary>
public class BackwardSchedulingServiceTests
{
    private static Part NewPart(
        string partNumber,
        int? leadTimeDays = null) => new()
        {
            PartNumber = partNumber,
            Name = partNumber,
            ProcurementSource = ProcurementSource.Buy,
            InventoryClass = InventoryClass.Component,
            Status = PartStatus.Active,
            LeadTimeDays = leadTimeDays,
        };

    [Fact]
    public async Task CalculateSchedule_BomLineWithNullLeadTime_FallsBackToPartSnapshot()
    {
        // Arrange — parent part with two Buy BOM children. One child carries
        // a per-line LeadTimeDays; the other has NULL on the line but its
        // Part snapshot tracks LeadTimeDays = 21. Pre-fix the service would
        // drop the null row entirely and report MaxLead = 5 → poOrderBy
        // 5 days before MaterialsNeededBy. With the fix it resolves to 21.
        using var db = TestDbContextFactory.Create();
        var parent = NewPart("PARENT");
        var childWithLine = NewPart("CHILD-LINE", leadTimeDays: 5);
        var childFromSnapshot = NewPart("CHILD-SNAP", leadTimeDays: 21);
        db.Parts.AddRange(parent, childWithLine, childFromSnapshot);
        await db.SaveChangesAsync();

        db.BOMEntries.AddRange(
            new BOMEntry
            {
                ParentPartId = parent.Id,
                ChildPartId = childWithLine.Id,
                Quantity = 1,
                SourceType = BOMSourceType.Buy,
                LeadTimeDays = 5,
            },
            new BOMEntry
            {
                ParentPartId = parent.Id,
                ChildPartId = childFromSnapshot.Id,
                Quantity = 1,
                SourceType = BOMSourceType.Buy,
                LeadTimeDays = null, // ← The fall-back trigger
            });

        var customer = new Customer { Name = "ACME" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var so = new SalesOrder
        {
            OrderNumber = "SO-1",
            CustomerId = customer.Id,
            Status = SalesOrderStatus.Confirmed,
            RequestedDeliveryDate = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
        };
        db.SalesOrders.Add(so);
        await db.SaveChangesAsync();

        var soLine = new SalesOrderLine
        {
            SalesOrderId = so.Id,
            PartId = parent.Id,
            Quantity = 1,
            UnitPrice = 100m,
            LineNumber = 1,
        };
        db.SalesOrderLines.Add(soLine);
        await db.SaveChangesAsync();

        var clock = new SystemClock();
        var resolver = new PartSourcingResolver(db);
        var service = new BackwardSchedulingService(db, clock, resolver);

        // Act
        var schedule = await service.CalculateSchedule(soLine.Id, CancellationToken.None);

        // Assert — poOrderBy is 21 days before MaterialsNeededBy, not 5.
        var leadDays = (schedule.MaterialsNeededBy - schedule.PoOrderBy).TotalDays;
        leadDays.Should().BeApproximately(21d, precision: 0.001d);
    }
}
