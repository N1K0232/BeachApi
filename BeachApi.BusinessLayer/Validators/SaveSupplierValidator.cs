using BeachApi.Shared.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validators;

internal class SaveSupplierValidator : AbstractValidator<SaveSupplierRequest>
{
    public SaveSupplierValidator()
    {
        RuleFor(s => s.CompanyName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("insert a valid company name");

        RuleFor(s => s.ContactName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("insert a valid contact name");

        RuleFor(s => s.City)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("insert a valid city");
    }
}