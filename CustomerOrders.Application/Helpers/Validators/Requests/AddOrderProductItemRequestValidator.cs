using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class AddOrderProductItemRequestValidator : AbstractValidator<AddOrderProductItemRequest>
    {
        public AddOrderProductItemRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThanOrEqualTo(0).WithMessage("ProductId cannot be negative.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
