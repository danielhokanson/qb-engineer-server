using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs.Parts;

public record UpdateJobPartCommand(
    int JobId,
    int JobPartId,
    decimal Quantity,
    string? Notes) : IRequest<JobPartResponseModel>;

public class UpdateJobPartHandler(AppDbContext db) : IRequestHandler<UpdateJobPartCommand, JobPartResponseModel>
{
    public async Task<JobPartResponseModel> Handle(UpdateJobPartCommand request, CancellationToken cancellationToken)
    {
        var jobPart = await db.JobParts
            .Include(jp => jp.Part)
            .FirstOrDefaultAsync(jp => jp.Id == request.JobPartId && jp.JobId == request.JobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job part with ID {request.JobPartId} not found.");

        jobPart.Quantity = request.Quantity;
        jobPart.Notes = request.Notes;

        await db.SaveChangesAsync(cancellationToken);

        return new JobPartResponseModel(
            jobPart.Id,
            jobPart.JobId,
            jobPart.PartId,
            jobPart.Part.PartNumber,
            jobPart.Part.Description,
            jobPart.Part.Status.ToString(),
            jobPart.Quantity,
            jobPart.Notes);
    }
}
