using MediatR;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record DeleteJobNoteCommand(int NoteId) : IRequest;

public class DeleteJobNoteHandler(AppDbContext db) : IRequestHandler<DeleteJobNoteCommand>
{
    public async Task Handle(DeleteJobNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await db.JobNotes.FindAsync([request.NoteId], cancellationToken)
            ?? throw new KeyNotFoundException($"Note {request.NoteId} not found");

        db.JobNotes.Remove(note);
        await db.SaveChangesAsync(cancellationToken);
    }
}
