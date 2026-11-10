using FluentValidation;
using Web.DTOs.Workout;

namespace Web.Validators;

public class WorkoutSetRequestValidator : AbstractValidator<WorkoutSetRequest>
{
    public WorkoutSetRequestValidator()
    {
        RuleFor(s => s.Repeats)
            .GreaterThanOrEqualTo(0).WithErrorCode("invalid_repeats")
            .LessThanOrEqualTo(ValidationConstants.MaxSetRepeats).WithErrorCode("too_many_repeats");

        RuleFor(s => s.Weight)
            .GreaterThanOrEqualTo(0).WithErrorCode("invalid_weight")
            .LessThanOrEqualTo(ValidationConstants.MaxSetWeight).WithErrorCode("too_heavy_weight");
    }
}
