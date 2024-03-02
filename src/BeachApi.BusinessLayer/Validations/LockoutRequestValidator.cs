using BeachApi.Shared.Models.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validations;

public class LockoutRequestValidator : AbstractValidator<LockoutRequest>
{
    public LockoutRequestValidator()
    {
        RuleFor(l => l.UserId)
            .NotEmpty()
            .WithMessage("the user is required");

        RuleFor(l => l.LockoutEnd)
            .NotEmpty()
            .WithMessage("the lockout end is required");
    }
}