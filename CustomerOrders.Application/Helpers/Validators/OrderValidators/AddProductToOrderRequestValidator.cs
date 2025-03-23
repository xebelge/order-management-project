using CustomerOrders.Application.DTOs.RequestDtos;
using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.OrderValidators
{
    public class AddProductToOrderRequestValidator : AbstractValidator<AddProductToOrderRequest>
    {
        public AddProductToOrderRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
