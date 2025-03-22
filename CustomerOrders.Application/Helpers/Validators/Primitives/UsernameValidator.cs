using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Primitives
{
    public class UsernameValidator : AbstractValidator<string>
    {
        public UsernameValidator()
        {
            RuleFor(username => username)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");
        }
    }
}
