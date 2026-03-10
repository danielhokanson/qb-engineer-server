using Microsoft.AspNetCore.DataProtection;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Services;

public class TokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;

    public TokenEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("QbEngineer.OAuthTokens");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return _protector.Unprotect(cipherText);
    }
}
