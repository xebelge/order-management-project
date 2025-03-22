using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Primitives
{
    public class EmailValidator : AbstractValidator<string>
    {
        public EmailValidator()
        {
            RuleFor(email => email)
                .NotEmpty().WithMessage("Email cannot be empty.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
