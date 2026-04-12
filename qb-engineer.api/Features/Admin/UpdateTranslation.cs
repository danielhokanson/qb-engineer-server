using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Admin;

public record UpdateTranslationCommand(string LanguageCode, string Key, UpdateTranslationRequestModel Request) : IRequest;

public class UpdateTranslationHandler(AppDbContext db, IClock clock) : IRequestHandler<UpdateTranslationCommand>
{
    public async Task Handle(UpdateTranslationCommand command, CancellationToken cancellationToken)
    {
        var existing = await db.TranslatedLabels
            .FirstOrDefaultAsync(l => l.LanguageCode == command.LanguageCode && l.Key == command.Key, cancellationToken);

        if (existing is not null)
        {
            existing.Value = command.Request.Value;
            existing.TranslatedAt = clock.UtcNow;
        }
        else
        {
            db.TranslatedLabels.Add(new TranslatedLabel
            {
                Key = command.Key,
                LanguageCode = command.LanguageCode,
                Value = command.Request.Value,
                TranslatedAt = clock.UtcNow,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
