using Domain.Models.Auth;

namespace Web.Presentation.DTOs;

public record UserResponse(Guid Id, string Email, Role Role, DateTime CreatedAt);
