using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs.Parts;

public record AddJobPartCommand(
    int JobId,
    int PartId,
    decimal Quantity = 1,
    string? Notes = null) : IRequest<JobPartResponseModel>;

public class AddJobPartHandler(AppDbContext db) : IRequestHandler<AddJobPartCommand, JobPartResponseModel>
{
    public async Task<JobPartResponseModel> Handle(AddJobPartCommand request, CancellationToken cancellationToken)
    {
        var job = await db.Jobs.FindAsync([request.JobId], cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var part = await db.Parts.FindAsync([request.PartId], cancellationToken)
            ?? throw new KeyNotFoundException($"Part with ID {request.PartId} not found.");

        var exists = await db.JobParts.AnyAsync(
            jp => jp.JobId == request.JobId && jp.PartId == request.PartId,
            cancellationToken);
        if (exists)
            throw new InvalidOperationException("This part is already linked to this job.");

        var jobPart = new JobPart
        {
            JobId = request.JobId,
            PartId = request.PartId,
            Quantity = request.Quantity,
            Notes = request.Notes,
        };

        db.JobParts.Add(jobPart);
        await db.SaveChangesAsync(cancellationToken);

        return new JobPartResponseModel(
            jobPart.Id,
            jobPart.JobId,
            jobPart.PartId,
            part.PartNumber,
            part.Description,
            part.Status.ToString(),
            jobPart.Quantity,
            jobPart.Notes);
    }
}
