using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.OrderValidators
{
    public class UpdateOrderAddressRequestValidator : AbstractValidator<UpdateOrderAddressRequest>
    {
        public UpdateOrderAddressRequestValidator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(512).WithMessage("Address cannot exceed 512 characters.");
        }
    }
}
