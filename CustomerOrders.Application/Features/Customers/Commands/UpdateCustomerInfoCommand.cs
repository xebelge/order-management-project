using MediatR;

public class UpdateCustomerInfoCommand : IRequest<bool>
{
    public string Username { get; set; } = null!;
    public UpdateCustomerInfoRequest Request { get; set; } = null!;
}
