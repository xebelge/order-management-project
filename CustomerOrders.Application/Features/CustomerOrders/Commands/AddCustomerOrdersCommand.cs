using CustomerOrders.Application.DTOs.RequestDtos;
using MediatR;

public class AddCustomerOrdersCommand : IRequest<AddCustomerOrdersResult>
{
    public List<CreateOrderRequest> Orders { get; set; } = new();
}
