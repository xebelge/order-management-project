using CustomerOrders.Application.DTOs.RequestDtos;
using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.OrderValidators
{
    public class UpdateOrderProductQuantityRequestValidator : AbstractValidator<UpdateOrderProductQuantityRequest>
    {
        public UpdateOrderProductQuantityRequestValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Order ID must be greater than 0.");
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product ID must be greater than 0.");
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative.");
        }
    }
}
