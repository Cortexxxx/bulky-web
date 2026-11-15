using Domain.Models.Auth;

namespace Web.DTOs.Auth;

public record UserResponse(Guid Id, string Username, string Email, Role Role, DateTime CreatedAt);
