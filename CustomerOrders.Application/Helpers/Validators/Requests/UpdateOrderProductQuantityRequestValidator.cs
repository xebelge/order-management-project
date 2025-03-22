using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class UpdateOrderProductQuantityRequestValidator : AbstractValidator<UpdateOrderProductQuantityRequest>
    {
        public UpdateOrderProductQuantityRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
