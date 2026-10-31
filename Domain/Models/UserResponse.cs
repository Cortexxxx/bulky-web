namespace Domain.Models;

public record UserResponse(Guid Id, string Email, Role Role, DateTime CreatedAt);