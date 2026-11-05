using FluentValidation;
using Web.DTOs;

namespace Web.Validators;

public class MuscleGroupRequestValidator : AbstractValidator<MuscleGroupRequest>
{
    public MuscleGroupRequestValidator()
    {
        RuleFor(m => m.Title)
            .NotEmpty().WithErrorCode("empty_title")
            .MinimumLength(ValidationConstants.MinMuscleTitleLength).WithErrorCode("short_title")
            .MaximumLength(ValidationConstants.MaxMuscleTitleLength).WithErrorCode("long_title")
            .Matches(@"[\p{L}\s]+");

        RuleFor(m => m.Description)
            .NotNull().WithErrorCode("null_description")
            .MaximumLength(500).WithErrorCode("long_description");
    }
}