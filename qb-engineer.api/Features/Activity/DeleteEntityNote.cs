using MediatR;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Activity;

public record DeleteEntityNoteCommand(int NoteId) : IRequest;

public class DeleteEntityNoteHandler(AppDbContext db) : IRequestHandler<DeleteEntityNoteCommand>
{
    public async Task Handle(DeleteEntityNoteCommand request, CancellationToken ct)
    {
        var note = await db.EntityNotes.FindAsync([request.NoteId], ct)
            ?? throw new KeyNotFoundException($"Note {request.NoteId} not found");

        db.EntityNotes.Remove(note);
        await db.SaveChangesAsync(ct);
    }
}
