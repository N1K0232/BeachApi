using BeachApi.Shared.Models.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validations;

public class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(p => p.Name)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("The product name is required");

        RuleFor(p => p.Description)
            .MaximumLength(512);

        RuleFor(p => p.Price)
            .PrecisionScale(8, 2, true)
            .WithMessage("Insert a valid price")
            .GreaterThan(0)
            .WithMessage("unable to create a product with 0 or negative price");

        RuleFor(p => p.Quantity)
            .GreaterThan(0)
            .When(p => p.Quantity != null)
            .WithMessage("can't add a product with 0 quantity");
    }
}