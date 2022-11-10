using BeachApi.Shared.Requests;
using FluentValidation;

namespace BeachApi.BusinessLayer.Validators;

internal class SaveInvoiceValidator : AbstractValidator<SaveInvoiceRequest>
{
    public SaveInvoiceValidator()
    {
        RuleFor(i => i.Title)
            .NotNull()
            .NotEmpty()
            .WithMessage("The title is required");

        RuleFor(i => i.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("The description is required");

        RuleFor(i => i.InvoiceDate)
            .NotEmpty()
            .WithMessage("Insert a valid date");

        RuleFor(i => i.Price)
            .NotEmpty()
            .WithMessage("Insert a valid price");
    }
}