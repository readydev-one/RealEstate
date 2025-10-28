// RealEstate.Application/Interfaces/IEncryptionService.cs
namespace RealEstate.Application.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}