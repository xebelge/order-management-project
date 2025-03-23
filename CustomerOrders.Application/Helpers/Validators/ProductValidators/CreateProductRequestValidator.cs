using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.ProductValidators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Barcode)
                .NotEmpty().WithMessage("Barcode is required.")
                .MaximumLength(64).WithMessage("Barcode must be at most 64 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(256).WithMessage("Description must be at most 256 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
