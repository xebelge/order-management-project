using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThanOrEqualTo(0).WithMessage("Customer ID cannot be negative.");

            RuleFor(x => x.ProductItems)
                .NotEmpty().WithMessage("At least one product must be included in the order.")
                .ForEach(x => x.SetValidator(new AddOrderProductItemRequestValidator()));
        }
    }
}
