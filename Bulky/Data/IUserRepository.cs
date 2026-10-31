using Bulky.Models;

namespace Bulky.Data;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct);
    Task<Guid> CreateUserAsync(string email, string passwordHash, string passwordSalt, CancellationToken ct);
}