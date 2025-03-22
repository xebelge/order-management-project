using CustomerOrders.Application.DTOs.RequestDtos;
using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequest>
    {
        public UpdatePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password cannot be empty.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password cannot be empty.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
        }
    }
}
