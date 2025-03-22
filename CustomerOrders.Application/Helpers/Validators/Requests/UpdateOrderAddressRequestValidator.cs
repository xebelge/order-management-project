using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class UpdateOrderAddressRequestValidator : AbstractValidator<UpdateOrderAddressRequest>
    {
        public UpdateOrderAddressRequestValidator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address cannot be empty.");
        }
    }

}
