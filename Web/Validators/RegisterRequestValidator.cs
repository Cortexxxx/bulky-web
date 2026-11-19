using FluentValidation;
using Web.DTOs;
using Web.DTOs.Auth;

namespace Web.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(ValidationOptions options)
    {
        RuleFor(r => r.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithErrorCode("empty_username")
            .Must(username => username.EnumerateRunes().Count() >= 3).WithErrorCode("short_username")
            .Must(username => username.EnumerateRunes().Count() <= 256).WithErrorCode("long_username");

        RuleFor(r => r.Password)
            .NotEmpty().WithErrorCode("empty_password")
            .MinimumLength(options.MinPasswordLength).WithErrorCode("short_password")
            .MaximumLength(options.MaxPasswordLength).WithErrorCode("long_password");

        RuleFor(r => r.Email)
            .NotEmpty().WithErrorCode("empty_email")
            .MaximumLength(254).WithErrorCode("long_email")
            .Matches(@"\A[^@\s]+@[^@\s]+\.[^@\s]+\z").WithErrorCode("incorrect_email");
    }
}
