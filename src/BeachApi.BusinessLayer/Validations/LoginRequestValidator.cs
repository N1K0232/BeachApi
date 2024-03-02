using BeachApi.Shared.Models.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validations;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.UserName)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("UserName is required");

        RuleFor(l => l.Password)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("The Password is required");
    }
}