using System.Text.Json;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record UpdatePartCommand(int Id, UpdatePartRequestModel Data) : IRequest<PartDetailResponseModel>;

public class UpdatePartCommandValidator : AbstractValidator<UpdatePartCommand>
{
    public UpdatePartCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.Description).MaximumLength(500).When(x => x.Data.Description is not null);
        RuleFor(x => x.Data.Revision).MaximumLength(10).When(x => x.Data.Revision is not null);
        RuleFor(x => x.Data.Material).MaximumLength(200).When(x => x.Data.Material is not null);
        RuleFor(x => x.Data.ExternalPartNumber).MaximumLength(100).When(x => x.Data.ExternalPartNumber is not null);
    }
}

public class UpdatePartHandler(
    IPartRepository repo,
    ISyncQueueRepository syncQueue,
    IAccountingProviderFactory providerFactory,
    ILogger<UpdatePartHandler> logger) : IRequestHandler<UpdatePartCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.Id} not found");

        var data = request.Data;

        if (data.Description is not null) part.Description = data.Description.Trim();
        if (data.Revision is not null) part.Revision = data.Revision.Trim();
        if (data.Status.HasValue) part.Status = data.Status.Value;
        if (data.PartType.HasValue) part.PartType = data.PartType.Value;
        if (data.Material is not null) part.Material = data.Material.Trim();
        if (data.MoldToolRef is not null) part.MoldToolRef = data.MoldToolRef.Trim();
        if (data.ExternalPartNumber is not null) part.ExternalPartNumber = data.ExternalPartNumber.Trim();
        if (data.ToolingAssetId.HasValue) part.ToolingAssetId = data.ToolingAssetId.Value == 0 ? null : data.ToolingAssetId.Value;
        if (data.PreferredVendorId.HasValue) part.PreferredVendorId = data.PreferredVendorId.Value == 0 ? null : data.PreferredVendorId.Value;
        if (data.MinStockThreshold.HasValue) part.MinStockThreshold = data.MinStockThreshold.Value == 0 ? null : data.MinStockThreshold.Value;
        if (data.ReorderPoint.HasValue) part.ReorderPoint = data.ReorderPoint.Value == 0 ? null : data.ReorderPoint.Value;
        if (data.ReorderQuantity.HasValue) part.ReorderQuantity = data.ReorderQuantity.Value == 0 ? null : data.ReorderQuantity.Value;
        if (data.LeadTimeDays.HasValue) part.LeadTimeDays = data.LeadTimeDays.Value == 0 ? null : data.LeadTimeDays.Value;
        if (data.SafetyStockDays.HasValue) part.SafetyStockDays = data.SafetyStockDays.Value;

        await repo.SaveChangesAsync(cancellationToken);

        // Enqueue QB Item update if part is linked and accounting is connected
        try
        {
            var accountingService = await providerFactory.GetActiveProviderAsync(cancellationToken);
            if (accountingService is not null)
            {
                var syncStatus = await accountingService.GetSyncStatusAsync(cancellationToken);
                if (syncStatus.Connected && part.ExternalId is not null)
                {
                    var item = new AccountingItem(
                        part.ExternalId, part.PartNumber, part.Description,
                        "NonInventory", null, null, part.PartNumber, part.Status == Core.Enums.PartStatus.Active);
                    var payload = JsonSerializer.Serialize(item);
                    await syncQueue.EnqueueAsync("Part", part.Id, "UpdateItem", payload, cancellationToken);
                    logger.LogInformation("Enqueued UpdateItem sync for Part {PartId}", part.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to enqueue item sync for Part {PartId} — continuing", part.Id);
        }

        return (await repo.GetDetailAsync(part.Id, cancellationToken))!;
    }
}
