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
            .Equal(500m)
            .WithMessage("Contribution amount must be exactly R500.");
    }
}