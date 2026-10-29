using Bulky.Models;

namespace Bulky.Data;

public interface ISessionRepository
{
    Task<Guid> CreateSessionAsync(Guid userId, string tokenHash, DateTime expiresAt, CancellationToken ct);
    Task<Session?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct);
    Task<bool> UpdateSessionTimeAsync(Guid id, DateTime expiresAt, CancellationToken ct);
    Task<bool> DeleteUserSessionsAsync(Guid userId, CancellationToken ct);
    Task<bool> DeleteSessionByIdAsync(Guid id, CancellationToken ct);
    Task<bool> DeleteExpiredSessionsAsync(CancellationToken ct);
}