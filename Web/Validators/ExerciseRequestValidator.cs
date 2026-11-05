using FluentValidation;
using Web.DTOs;

namespace Web.Validators;

public class ExerciseRequestValidator : AbstractValidator<ExerciseRequest>
{
    public ExerciseRequestValidator()
    {
        RuleFor(e => e.Title)
            .NotEmpty().WithErrorCode("empty_title")
            .MinimumLength(3).WithErrorCode("short_title")
            .MaximumLength(256).WithErrorCode("long_title");
        RuleFor(e => e.Description)
            .NotNull().WithErrorCode("null_description")
            .MaximumLength(500).WithErrorCode("long_description");
        RuleFor(e => e.MuscleGroups)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithErrorCode("empty_muscles")
            .Must(m => m.Count == m
                .Select(em => em.MuscleGroupId)
                .Distinct()
                .Count())
            .WithErrorCode("muscle_repeat");
        RuleForEach(e => e.MuscleGroups)
            .NotNull().WithErrorCode("null_muscle")
            .ChildRules(muscle =>
            {
                muscle.RuleFor(m => m.MuscleGroupId).NotEmpty().WithErrorCode("empty_muscle_id");
                muscle.RuleFor(m => m.MuscleGroupRole).IsInEnum().WithErrorCode("invalid_role");
            });
    }
}