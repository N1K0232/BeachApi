using BeachApi.Shared.Models.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validations;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the first name is required");

        RuleFor(r => r.LastName)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the last name is required");

        RuleFor(r => r.Email)
            .EmailAddress()
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the email is required");

        RuleFor(r => r.Password)
            .Length(8, 50)
            .NotEmpty()
            .WithMessage("invalid password");
    }
}