using FluentValidation;

namespace CustomerOrders.Application.Helpers.Validators.CustomerValidators
{
    public class UpdateCustomerInfoRequestValidator : AbstractValidator<UpdateCustomerInfoRequest>
    {
        public UpdateCustomerInfoRequestValidator()
        {
            RuleFor(x => x)
                .Must(HaveAtLeastOneField)
                .WithMessage("At least one field (Name, NewEmail, or Address) must be provided.");

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .MinimumLength(2).WithMessage("Name must be at least 2 characters long.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.NewEmail), () =>
            {
                RuleFor(x => x.OldEmail)
                    .NotEmpty().WithMessage("Old email is required when changing email.")
                    .EmailAddress().WithMessage("Old email must be a valid email.");

                RuleFor(x => x.NewEmail)
                    .NotEmpty().WithMessage("New email is required.")
                    .EmailAddress().WithMessage("New email must be a valid email.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Address), () =>
            {
                RuleFor(x => x.Address)
                    .MinimumLength(5).WithMessage("Address must be at least 5 characters long.");
            });
        }

        private bool HaveAtLeastOneField(UpdateCustomerInfoRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.Name)
                || !string.IsNullOrWhiteSpace(request.NewEmail)
                || !string.IsNullOrWhiteSpace(request.Address);
        }
    }
}
