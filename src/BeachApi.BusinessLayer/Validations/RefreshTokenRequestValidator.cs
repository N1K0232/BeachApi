using BeachApi.Shared.Models.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validations;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(r => r.AccessToken)
            .NotEmpty()
            .WithMessage("the access token is required");

        RuleFor(r => r.RefreshToken)
            .NotEmpty()
            .WithMessage("the refresh token is required");
    }
}