namespace Domain.Models.Auth;

public record struct User()
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public Role Role {get;init;}
    public string PasswordHash { get; init; }
    public string PasswordSalt { get; init; }
    public DateTime CreatedAt { get; init; }
}