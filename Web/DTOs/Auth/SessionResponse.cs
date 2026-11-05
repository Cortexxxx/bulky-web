namespace Web.DTOs.Auth;

public record SessionResponse(Guid Id, DateTime CreatedAt, DateTime ExpiresAt);