using System.Text.Json;

using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record CreatePartCommand(
    string Description,
    string? Revision,
    PartType PartType,
    string? Material,
    string? MoldToolRef,
    string? ExternalPartNumber) : IRequest<PartDetailResponseModel>;

public class CreatePartCommandValidator : AbstractValidator<CreatePartCommand>
{
    public CreatePartCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Revision).MaximumLength(10).When(x => x.Revision is not null);
        RuleFor(x => x.Material).MaximumLength(200).When(x => x.Material is not null);
        RuleFor(x => x.ExternalPartNumber).MaximumLength(100).When(x => x.ExternalPartNumber is not null);
    }
}

public class CreatePartHandler(
    IPartRepository repo,
    ISyncQueueRepository syncQueue,
    IAccountingProviderFactory providerFactory,
    ILogger<CreatePartHandler> logger) : IRequestHandler<CreatePartCommand, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        var partNumber = await repo.GetNextPartNumberAsync(request.PartType, cancellationToken);

        var part = new Part
        {
            PartNumber = partNumber,
            Description = request.Description.Trim(),
            Revision = request.Revision?.Trim() ?? "A",
            PartType = request.PartType,
            Status = PartStatus.Draft,
            Material = request.Material?.Trim(),
            MoldToolRef = request.MoldToolRef?.Trim(),
            ExternalPartNumber = request.ExternalPartNumber?.Trim(),
        };

        await repo.AddAsync(part, cancellationToken);

        // Enqueue QB Item creation if accounting is connected
        try
        {
            var accountingService = await providerFactory.GetActiveProviderAsync(cancellationToken);
            if (accountingService is not null)
            {
                var syncStatus = await accountingService.GetSyncStatusAsync(cancellationToken);
                if (syncStatus.Connected)
                {
                    var item = new AccountingItem(
                        null, part.PartNumber, part.Description,
                        "NonInventory", null, null, part.PartNumber, true);
                    var payload = JsonSerializer.Serialize(item);
                    await syncQueue.EnqueueAsync("Part", part.Id, "CreateItem", payload, cancellationToken);
                    logger.LogInformation("Enqueued CreateItem sync for Part {PartId}", part.Id);
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
