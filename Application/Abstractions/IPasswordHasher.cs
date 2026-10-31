namespace Application.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string password, string passwordSalt);
    byte[] HashPasswordInBytes(string password, string passwordSalt);
    bool CheckPassword(string potentialPassword, string passwordHash, string passwordSalt);
    string HashToken(string token);
    string GenerateToken();
    string GenerateSalt();
}