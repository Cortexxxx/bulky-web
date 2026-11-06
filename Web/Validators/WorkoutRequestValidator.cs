using FluentValidation;
using Web.DTOs;
using Web.DTOs.Workout;

namespace Web.Validators;

public class WorkoutRequestValidator : AbstractValidator<WorkoutRequest>
{
    public WorkoutRequestValidator()
    {
        RuleFor(w => w.Title)
            .NotEmpty().WithErrorCode("empty_title")
            .MaximumLength(256).WithErrorCode("long_title");
    }
}
