using CustomerOrders.Application.DTOs;
using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.ProductValidators
{
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

            RuleFor(x => x)
                .Must(HaveAtLeastOneUpdatableField)
                .WithMessage("At least one field (Barcode, Description, Price, Quantity) must be provided for update.");

            When(x => !string.IsNullOrWhiteSpace(x.Barcode), () =>
            {
                RuleFor(x => x.Barcode)
                    .MaximumLength(64).WithMessage("Barcode must be at most 64 characters.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(256).WithMessage("Description must be at most 256 characters.");
            });

            When(x => x.Price.HasValue, () =>
            {
                RuleFor(x => x.Price.Value)
                    .GreaterThan(0).WithMessage("Price must be greater than zero.");
            });

            When(x => x.Quantity.HasValue, () =>
            {
                RuleFor(x => x.Quantity.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
            });
        }

        private bool HaveAtLeastOneUpdatableField(UpdateProductRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.Barcode)
                   || !string.IsNullOrWhiteSpace(request.Description)
                   || request.Price.HasValue
                   || request.Quantity.HasValue;
        }
    }
}
