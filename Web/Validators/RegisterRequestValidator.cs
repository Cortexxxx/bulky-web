using FluentValidation;
using Web.DTOs;
using Web.DTOs.Auth;

namespace Web.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.Password)
            .NotEmpty().WithErrorCode("empty_password")
            .MinimumLength(ValidationConstants.MinPasswordLength).WithErrorCode("short_password")
            .MaximumLength(ValidationConstants.MaxPasswordLength).WithErrorCode("long_password");

        RuleFor(r => r.Email)
            .NotEmpty().WithErrorCode("empty_email")
            .MaximumLength(254).WithErrorCode("long_email")
            .Matches(@"\A[^@\s]+@[^@\s]+\.[^@\s]+\z").WithErrorCode("incorrect_email");
    }
}