using FluentValidation;
using RondiTrack.DTOs.Stokvels;

namespace RondiTrack.Validators;

public sealed class CreateStokvelRequestValidator
    : AbstractValidator<CreateStokvelRequest>
{
    public CreateStokvelRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.MonthlyContribution)
            .Equal(500m)
            .WithMessage("Monthly contribution must be exactly R500.");
    }
}