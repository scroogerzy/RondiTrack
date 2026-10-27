using FluentValidation;
using RondiTrack.DTOs.Contributions;

namespace RondiTrack.Validators;

public sealed class RecordContributionRequestValidator
    : AbstractValidator<RecordContributionRequest>
{
    public RecordContributionRequestValidator()
    {
        RuleFor(x => x.Cycle)
            .GreaterThan(0)
            .WithMessage("Contribution cycle must be greater than zero.");

        RuleFor(x => x.Amount)
            .GreaterThan(0m)
            .WithMessage("Contribution amount must be greater than zero.");

        RuleFor(x => x.Amount)
            .Must(amount => decimal.Round(amount, 2, MidpointRounding.AwayFromZero) == amount)
            .WithMessage("Contribution amount must not contain fractions of a cent.");
    }
}