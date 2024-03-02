using BeachApi.Shared.Models.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validations;

public class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(c => c.Name)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("The name is required");

        RuleFor(c => c.Description)
            .MaximumLength(512);
    }
}