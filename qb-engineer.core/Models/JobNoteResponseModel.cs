namespace QBEngineer.Core.Models;

public record JobNoteResponseModel(
    int Id,
    string Text,
    string AuthorName,
    string AuthorInitials,
    string AuthorColor,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
