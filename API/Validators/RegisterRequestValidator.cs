using FluentValidation;
using RondiTrack.DTOs.Auth;

namespace RondiTrack.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(12);
    }
}
