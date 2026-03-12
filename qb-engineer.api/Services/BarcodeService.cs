using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Services;

public class BarcodeService(AppDbContext db) : IBarcodeService
{
    private static readonly Dictionary<BarcodeEntityType, string> Prefixes = new()
    {
        [BarcodeEntityType.User] = "EMP",
        [BarcodeEntityType.Part] = "PRT",
        [BarcodeEntityType.Job] = "JOB",
        [BarcodeEntityType.SalesOrder] = "SO",
        [BarcodeEntityType.PurchaseOrder] = "PO",
        [BarcodeEntityType.Asset] = "AST",
        [BarcodeEntityType.StorageLocation] = "LOC",
    };

    public async Task<Barcode> CreateBarcodeAsync(
        BarcodeEntityType entityType, int entityId, string naturalIdentifier,
        CancellationToken cancellationToken = default)
    {
        var prefix = Prefixes[entityType];
        var value = $"{prefix}-{naturalIdentifier}";

        // Ensure uniqueness — if a collision exists, append entity ID
        var exists = await db.Barcodes.AnyAsync(b => b.Value == value, cancellationToken);
        if (exists)
            value = $"{prefix}-{naturalIdentifier}-{entityId}";

        var barcode = new Barcode
        {
            Value = value,
            EntityType = entityType,
            IsActive = true,
        };

        // Set the appropriate FK
        switch (entityType)
        {
            case BarcodeEntityType.User:
                barcode.UserId = entityId;
                break;
            case BarcodeEntityType.Part:
                barcode.PartId = entityId;
                break;
            case BarcodeEntityType.Job:
                barcode.JobId = entityId;
                break;
            case BarcodeEntityType.SalesOrder:
                barcode.SalesOrderId = entityId;
                break;
            case BarcodeEntityType.PurchaseOrder:
                barcode.PurchaseOrderId = entityId;
                break;
            case BarcodeEntityType.Asset:
                barcode.AssetId = entityId;
                break;
            case BarcodeEntityType.StorageLocation:
                barcode.StorageLocationId = entityId;
                break;
        }

        db.Barcodes.Add(barcode);
        await db.SaveChangesAsync(cancellationToken);

        return barcode;
    }

    public async Task<Barcode?> FindByValueAsync(string value, CancellationToken cancellationToken = default)
    {
        return await db.Barcodes
            .FirstOrDefaultAsync(b => b.Value == value && b.IsActive, cancellationToken);
    }
}
