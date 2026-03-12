using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ShopFloor;

/// <summary>
/// Identifies a scanned value by checking the central Barcode table first,
/// then UserScanIdentifiers (auth-specific RFID/NFC), then legacy EmployeeBarcode field.
/// Returns the entity type and details so the kiosk can route accordingly.
/// </summary>
public record IdentifyScanQuery(string ScanValue) : IRequest<ScanIdentificationResult>;

public record ScanIdentificationResult(
    string ScanType,
    int? EntityId = null,
    string? EntityNumber = null,
    string? EntityTitle = null,
    string? StageName = null,
    string? StageColor = null);

public class IdentifyScanHandler(
    AppDbContext db,
    UserManager<ApplicationUser> userManager) : IRequestHandler<IdentifyScanQuery, ScanIdentificationResult>
{
    public async Task<ScanIdentificationResult> Handle(IdentifyScanQuery request, CancellationToken cancellationToken)
    {
        var scanValue = request.ScanValue.Trim();

        // 1. Check central Barcode table (primary lookup)
        var barcode = await db.Barcodes
            .FirstOrDefaultAsync(b => b.Value == scanValue && b.IsActive, cancellationToken);

        if (barcode != null)
        {
            return barcode.EntityType switch
            {
                BarcodeEntityType.User => new ScanIdentificationResult("employee"),
                BarcodeEntityType.Job => await ResolveJobResult(barcode.JobId!.Value, cancellationToken),
                BarcodeEntityType.Part => new ScanIdentificationResult("part", barcode.PartId),
                BarcodeEntityType.SalesOrder => new ScanIdentificationResult("sales-order", barcode.SalesOrderId),
                BarcodeEntityType.PurchaseOrder => new ScanIdentificationResult("purchase-order", barcode.PurchaseOrderId),
                BarcodeEntityType.Asset => new ScanIdentificationResult("asset", barcode.AssetId),
                BarcodeEntityType.StorageLocation => new ScanIdentificationResult("storage-location", barcode.StorageLocationId),
                _ => new ScanIdentificationResult("unknown"),
            };
        }

        // 2. Check UserScanIdentifiers (RFID, NFC, barcode, biometric — auth-specific)
        var hasIdentifier = await db.UserScanIdentifiers
            .AnyAsync(x => x.IdentifierValue == scanValue && x.IsActive, cancellationToken);

        if (hasIdentifier)
            return new ScanIdentificationResult("employee");

        // 3. Fallback: ApplicationUser.EmployeeBarcode (legacy)
        var hasEmployeeBarcode = await userManager.Users
            .AnyAsync(u => u.EmployeeBarcode == scanValue && u.IsActive, cancellationToken);

        if (hasEmployeeBarcode)
            return new ScanIdentificationResult("employee");

        // 4. Unknown scan
        return new ScanIdentificationResult("unknown");
    }

    private async Task<ScanIdentificationResult> ResolveJobResult(int jobId, CancellationToken cancellationToken)
    {
        var job = await db.Jobs
            .Include(j => j.CurrentStage)
            .Where(j => j.Id == jobId && !j.IsArchived && j.CompletedDate == null)
            .Select(j => new
            {
                j.Id,
                j.JobNumber,
                j.Title,
                StageName = j.CurrentStage != null ? j.CurrentStage.Name : null,
                StageColor = j.CurrentStage != null ? j.CurrentStage.Color : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (job != null)
            return new ScanIdentificationResult("job", job.Id, job.JobNumber, job.Title, job.StageName, job.StageColor);

        // Job exists but is archived/completed — still report as job
        return new ScanIdentificationResult("job", jobId);
    }
}
