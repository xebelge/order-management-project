using CustomerOrders.Application.Services;
using MediatR;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly CustomerService _customerService;

    public DeleteCustomerCommandHandler(CustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetUserByUsernameAsync(request.Username);
        if (customer == null) return false;

        await _customerService.DeleteUserAsync(customer.Id);
        return true;
    }
}
