using Bulky.Data;
using Bulky.Exceptions;
using Bulky.Models;

namespace Bulky.Services;

public class AuthService
{
    private readonly string _fakeSalt;
    private readonly string _fakePassword;
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;

    public AuthService(IUserRepository userRepository, ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _fakeSalt = PasswordHasher.GenerateSalt();
        _fakePassword = PasswordHasher.HashPassword(PasswordHasher.GenerateSalt(), _fakeSalt);
    }

    public async Task<Guid> RegisterUserAsync(string email, string password, CancellationToken ct)
    {
        var passwordSalt = PasswordHasher.GenerateSalt();
        var passwordHash = PasswordHasher.HashPassword(password, passwordSalt);
        
        var createdUserId = await _userRepository.CreateUserAsync(email, passwordHash, passwordSalt, ct);

        return createdUserId;
    }
    
    public async Task<User> LoginUserAsync(string email, string password, CancellationToken ct)
    {
        var user = await _userRepository.GetUserByEmailAsync(email, ct);

        var isCorrectPassword = PasswordHasher.CheckPassword(password,
            user?.PasswordHash ?? _fakePassword, user?.PasswordSalt ?? _fakeSalt);

        if (user != null && isCorrectPassword)
        {
            return user.Value;
        }

        throw new WrongEmailOrPasswordException("Wrong email or password");
    }

    public async Task<(Guid id, string token)> CreateUserSessionAsync(Guid userId, CancellationToken ct)
    {
        var token = PasswordHasher.GenerateToken();
        var tokenHash = PasswordHasher.HashToken(token);
        
        var sessionId = await _sessionRepository.CreateSessionAsync(userId, 
            tokenHash, DateTime.UtcNow.AddMinutes(5), ct);

        return (sessionId, token);
    }
    
    public async Task<Session> GetUserSessionAsync(string token, CancellationToken ct)
    {
        var tokenHash = PasswordHasher.HashToken(token);
        
        var session = await _sessionRepository.GetSessionByTokenHashAsync(tokenHash, ct);
        
        if (session is not null)
        {
            return session.Value;
        }
        
        throw new NoActiveSessionsException();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepository.GetUserByIdAsync(userId, ct);
        return user;
    }
}