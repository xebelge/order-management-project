using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Primitives
{
    public class IdValidator : AbstractValidator<int>
    {
        public IdValidator()
        {
            RuleFor(id => id)
                .GreaterThanOrEqualTo(0).WithMessage("ID cannot be negative.");
        }
    }
}
