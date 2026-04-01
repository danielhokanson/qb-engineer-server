using System.Text.Json;

using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using QBEngineer.Api.Hubs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record MoveJobStageCommand(int JobId, int StageId) : IRequest<JobDetailResponseModel>;

public class MoveJobStageHandler(
    IJobRepository jobRepo,
    ITrackTypeRepository trackRepo,
    IActivityLogRepository actRepo,
    ICustomerRepository customerRepo,
    IAccountingService accountingService,
    ISyncQueueRepository syncQueue,
    IMediator mediator,
    IHubContext<BoardHub> boardHub,
    ILogger<MoveJobStageHandler> logger) : IRequestHandler<MoveJobStageCommand, JobDetailResponseModel>
{
    public async Task<JobDetailResponseModel> Handle(MoveJobStageCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepo.FindAsync(request.JobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var targetStage = await trackRepo.FindStageAsync(request.StageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Stage with ID {request.StageId} not found.");

        if (targetStage.TrackTypeId != job.TrackTypeId)
            throw new InvalidOperationException(
                $"Stage {request.StageId} does not belong to track type {job.TrackTypeId}.");

        var previousStage = await trackRepo.FindStageAsync(job.CurrentStageId, cancellationToken);
        var previousStageName = previousStage?.Name;
        var previousStageId = job.CurrentStageId;

        // Backward move enforcement: block moves away from irreversible stages
        if (previousStage is not null
            && previousStage.IsIrreversible
            && targetStage.SortOrder < previousStage.SortOrder)
        {
            throw new InvalidOperationException(
                $"Cannot move backward from irreversible stage '{previousStage.Name}'. " +
                "Documents created at this stage cannot be voided.");
        }

        job.CurrentStageId = request.StageId;

        var maxPosition = await jobRepo.GetMaxBoardPositionAsync(request.StageId, cancellationToken);
        job.BoardPosition = maxPosition + 1;

        var log = new JobActivityLog
        {
            JobId = job.Id,
            Action = ActivityAction.StageMoved,
            FieldName = "CurrentStageId",
            OldValue = previousStageName,
            NewValue = targetStage.Name,
            Description = $"Moved from {previousStageName} to {targetStage.Name}.",
        };
        await actRepo.AddAsync(log, cancellationToken);

        await jobRepo.SaveChangesAsync(cancellationToken);

        // Accounting document creation: enqueue if the target stage triggers a document
        if (targetStage.AccountingDocumentType.HasValue)
        {
            await TryEnqueueAccountingDocumentAsync(job, targetStage, cancellationToken);
        }

        var result = await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);

        // Broadcast to board group
        await boardHub.Clients.Group($"board:{job.TrackTypeId}")
            .SendAsync("jobMoved", new BoardJobMovedEvent(
                job.Id, previousStageId, request.StageId,
                targetStage.Name, job.BoardPosition), cancellationToken);

        return result;
    }

    private async Task TryEnqueueAccountingDocumentAsync(
        Job job,
        JobStage targetStage,
        CancellationToken cancellationToken)
    {
        // Only enqueue when an accounting provider is actually connected
        var isConnected = await accountingService.TestConnectionAsync(cancellationToken);
        if (!isConnected)
        {
            logger.LogDebug(
                "Accounting provider not connected — skipping document queue for Job {JobId} moving to stage {StageName}.",
                job.Id, targetStage.Name);
            return;
        }

        // Resolve the customer and verify it is linked to the accounting provider
        if (job.CustomerId is null)
        {
            logger.LogDebug(
                "Job {JobId} has no customer — skipping accounting document queue for stage {StageName}.",
                job.Id, targetStage.Name);
            return;
        }

        var customer = await customerRepo.FindAsync(job.CustomerId.Value, cancellationToken);
        if (customer is null || string.IsNullOrWhiteSpace(customer.ExternalId))
        {
            logger.LogDebug(
                "Customer {CustomerId} on Job {JobId} has no ExternalId — skipping accounting document queue.",
                job.CustomerId, job.Id);
            return;
        }

        var documentType = targetStage.AccountingDocumentType!.Value;
        var operation = $"Create{documentType}"; // e.g. "CreateEstimate", "CreateInvoice"

        var document = new AccountingDocument(
            Type: documentType,
            CustomerExternalId: customer.ExternalId,
            LineItems:
            [
                new AccountingLineItem(
                    Description: job.Title,
                    Quantity: 1,
                    UnitPrice: 0m,
                    ItemExternalId: null)
            ],
            RefNumber: job.JobNumber,
            Amount: 0m,
            Date: DateTimeOffset.UtcNow);

        var payload = JsonSerializer.Serialize(document);

        await syncQueue.EnqueueAsync("Job", job.Id, operation, payload, cancellationToken);

        var queueLog = new JobActivityLog
        {
            JobId = job.Id,
            Action = ActivityAction.StageMoved,
            FieldName = "AccountingSync",
            OldValue = null,
            NewValue = operation,
            Description = $"Queued {documentType} creation for QuickBooks.",
        };
        await actRepo.AddAsync(queueLog, cancellationToken);
        await jobRepo.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Enqueued accounting operation '{Operation}' for Job {JobId} (Customer ExternalId: {ExternalId}).",
            operation, job.Id, customer.ExternalId);
    }
}
