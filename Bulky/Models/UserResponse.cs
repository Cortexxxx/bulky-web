using Bulky.Router;

namespace Bulky.Models;

public record UserResponse(Guid Id, string Email, Role Role, DateTime CreatedAt);