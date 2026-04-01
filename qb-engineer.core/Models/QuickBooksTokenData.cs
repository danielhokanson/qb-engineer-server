namespace QBEngineer.Core.Models;

public record QuickBooksTokenData(
    string AccessToken,
    string RefreshToken,
    string RealmId,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
