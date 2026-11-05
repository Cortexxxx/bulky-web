namespace Web.DTOs.Auth;

public record struct LoginRequest(string Email, string Password);