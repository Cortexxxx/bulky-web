using Application.Abstractions;
using Application.Exceptions;
using Domain.Models.Auth;

namespace Application.Services;

public class AuthService
{
    private readonly string _fakeSalt;
    private readonly string _fakePassword;
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, ISessionRepository sessionRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _passwordHasher = passwordHasher;
        _fakeSalt = _passwordHasher.GenerateSalt();
        _fakePassword = _passwordHasher.HashPassword(_passwordHasher.GenerateSalt(), _fakeSalt);
    }

    public async Task<Guid> RegisterUserAsync(string email, string password, CancellationToken ct)
    {
        var passwordSalt = _passwordHasher.GenerateSalt();
        var passwordHash = _passwordHasher.HashPassword(password, passwordSalt);
        
        var createdUserId = await _userRepository.CreateUserAsync(email, passwordHash, passwordSalt, ct);

        return createdUserId;
    }
    
    public async Task<User> LoginUserAsync(string email, string password, CancellationToken ct)
    {
        var user = await _userRepository.GetUserByEmailAsync(email, ct);

        var isCorrectPassword = _passwordHasher.CheckPassword(password,
            user?.PasswordHash ?? _fakePassword, user?.PasswordSalt ?? _fakeSalt);

        if (user != null && isCorrectPassword)
        {
            return user.Value;
        }

        throw new WrongEmailOrPasswordException("Wrong email or password");
    }

    public async Task<(Guid id, string token)> CreateUserSessionAsync(Guid userId, CancellationToken ct)
    {
        var token = _passwordHasher.GenerateToken();
        var tokenHash = _passwordHasher.HashToken(token);
        
        var sessionId = await _sessionRepository.CreateSessionAsync(userId, 
            tokenHash, DateTime.UtcNow.AddMinutes(5), ct);

        return (sessionId, token);
    }
    
    public async Task<Session> GetUserSessionAsync(string token, CancellationToken ct)
    {
        var tokenHash = _passwordHasher.HashToken(token);
        
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