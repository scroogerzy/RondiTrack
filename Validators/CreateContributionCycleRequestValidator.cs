using FluentValidation;
using RondiTrack.DTOs.ContributionCycles;

namespace RondiTrack.Validators;

public sealed class CreateContributionCycleRequestValidator
    : AbstractValidator<CreateContributionCycleRequest>
{
    public CreateContributionCycleRequestValidator()
    {
        RuleFor(x => x.PeriodNumber)
            .GreaterThan(0)
            .WithMessage("Period number must be greater than zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty();

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0)
            .WithMessage("Target amount must be greater than zero.");
    }
}