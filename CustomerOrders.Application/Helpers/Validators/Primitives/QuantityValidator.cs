using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Primitives
{
    public class QuantityValidator : AbstractValidator<int>
    {
        public QuantityValidator()
        {
            RuleFor(q => q)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
