namespace QBEngineer.Core.Interfaces;

public interface ITokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
