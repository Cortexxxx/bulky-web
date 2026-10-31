namespace Domain.Models;

public record struct Session
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Token { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}