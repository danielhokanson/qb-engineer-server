namespace QBEngineer.Core.Models;

public record QuickBooksTokenData(
    string AccessToken,
    string RefreshToken,
    string RealmId,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
