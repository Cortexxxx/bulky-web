using System.Security.Cryptography;
using Application.Abstractions;

namespace Web.Persistence;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password, string passwordSalt) 
        => Convert.ToBase64String(HashPasswordInBytes(password, passwordSalt));

    public byte[] HashPasswordInBytes(string password, string passwordSalt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, 
            Convert.FromBase64String(passwordSalt), 600_000, 
            HashAlgorithmName.SHA256, 32);
    }

    public string GenerateSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    public bool CheckPassword(string potentialPassword, string passwordHash, string passwordSalt)
    {
        var potentialHash = HashPasswordInBytes(potentialPassword, passwordSalt);
        return CryptographicOperations.FixedTimeEquals(potentialHash, 
            Convert.FromBase64String(passwordHash));
    }

    public string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Convert.FromHexString(token)));
    }

    public string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
    }
}