using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.OrderValidators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer ID must be greater than 0.");

            RuleFor(x => x.ProductItems)
                .NotEmpty().WithMessage("Product list cannot be empty.")
                .Must(items => items.All(i => i.ProductId > 0 && i.Quantity > 0))
                .WithMessage("Each product must have a valid ProductId and Quantity.");
        }
    }
}
