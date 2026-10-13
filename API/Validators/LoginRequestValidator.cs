using FluentValidation;
using RondiTrack.DTOs.Auth;

namespace RondiTrack.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Password).NotEmpty();
    }
}
