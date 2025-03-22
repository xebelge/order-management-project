using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.Requests
{
    public class UpdateCustomerInfoRequestValidator : AbstractValidator<UpdateCustomerInfoRequest>
    {
        public UpdateCustomerInfoRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty.")
                .Length(2, 50).WithMessage("Name must be between 2 and 50 characters.")
                .Matches(@"^[\p{L} ]+$").WithMessage("Name must only contain letters and spaces.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address cannot be empty.");

            When(x => !string.IsNullOrWhiteSpace(x.NewEmail), () =>
            {
                RuleFor(x => x.NewEmail)
                    .EmailAddress().WithMessage("New email format is invalid.");
            });
        }
    }
}
