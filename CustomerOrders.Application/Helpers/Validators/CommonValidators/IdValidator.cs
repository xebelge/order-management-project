using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.CommonValidators
{
    public class IdValidator : AbstractValidator<int>
    {
        public IdValidator()
        {
            RuleFor(x => x)
                .GreaterThan(0).WithMessage("ID must be greater than 0.");
        }
    }
}
