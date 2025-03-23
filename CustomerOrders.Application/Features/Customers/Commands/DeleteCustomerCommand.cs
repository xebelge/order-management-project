using MediatR;

public class DeleteCustomerCommand : IRequest<bool>
{
    public string Username { get; set; } = null!;
}
