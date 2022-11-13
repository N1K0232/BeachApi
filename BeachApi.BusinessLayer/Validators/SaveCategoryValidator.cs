using BeachApi.Shared.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validators;

internal class SaveCategoryValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(256)
            .WithMessage("Insert a valid category name");
    }
}