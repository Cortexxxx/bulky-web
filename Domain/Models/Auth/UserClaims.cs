namespace Domain.Models.Auth;

public class UserClaims
{
    public Role Role { get; init; } = Role.User;
    public required string Email { get; init; }
    public required Guid Id { get; init; }
    public required DateTime CreatedAt { get; init; }
}