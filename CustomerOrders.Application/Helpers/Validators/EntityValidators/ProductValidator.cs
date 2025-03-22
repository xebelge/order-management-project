using CustomerOrders.Core.Entities;
using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.EntityValidators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(p => p.Barcode)
                .NotEmpty().WithMessage("Product Barcode cannot be empty.");

            RuleFor(p => p.Description)
                .NotEmpty().WithMessage("Product Description cannot be empty.");

            RuleFor(p => p.Quantity)
                .GreaterThan(0).WithMessage("Product Quantity must be greater than zero.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Product Price must be greater than zero.");
        }
    }
}
