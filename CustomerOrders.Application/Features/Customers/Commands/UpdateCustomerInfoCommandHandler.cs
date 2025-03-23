using CustomerOrders.Application.Services;
using FluentValidation;
using MediatR;

public class UpdateCustomerInfoCommandHandler : IRequestHandler<UpdateCustomerInfoCommand, bool>
{
    private readonly CustomerService _customerService;
    private readonly IValidator<string> _nameValidator;
    private readonly IValidator<string> _emailValidator;

    public UpdateCustomerInfoCommandHandler(
        CustomerService customerService,
        IValidator<string> nameValidator,
        IValidator<string> emailValidator)
    {
        _customerService = customerService;
        _nameValidator = nameValidator;
        _emailValidator = emailValidator;
    }

    public async Task<bool> Handle(UpdateCustomerInfoCommand command, CancellationToken cancellationToken)
    {
        var customerDto = await _customerService.GetUserByUsernameAsync(command.Username);
        if (customerDto == null) return false;

        var req = command.Request;

        if (!string.IsNullOrWhiteSpace(req.Name))
        {
            var nameResult = _nameValidator.Validate(req.Name);
            if (!nameResult.IsValid)
                throw new ValidationException(nameResult.Errors);
            customerDto.Name = req.Name;
        }

        if (!string.IsNullOrWhiteSpace(req.Address))
            customerDto.Address = req.Address;

        if (!string.IsNullOrWhiteSpace(req.OldEmail) && !string.IsNullOrWhiteSpace(req.NewEmail))
        {
            var currentEmail = customerDto.Email.Trim().ToLowerInvariant();
            var oldEmail = req.OldEmail.Trim().ToLowerInvariant();
            var newEmail = req.NewEmail.Trim().ToLowerInvariant();

            if (currentEmail != oldEmail)
                throw new ValidationException("Old email does not match current.");

            var existingUser = await _customerService.GetUserByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != customerDto.Id)
                throw new ValidationException("New email is already taken.");

            var emailResult = _emailValidator.Validate(newEmail);
            if (!emailResult.IsValid)
                throw new ValidationException(emailResult.Errors);

            customerDto.Email = newEmail;
        }

        await _customerService.UpdateUserAsync(customerDto);
        return true;
    }
}
