using CustomerOrders.Application.DTOs.EntityDtos;
using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username cannot be empty.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.");
        }
    }
}
