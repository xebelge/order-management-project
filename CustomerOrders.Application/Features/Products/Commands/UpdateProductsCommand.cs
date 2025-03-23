using CustomerOrders.Application.DTOs;
using MediatR;

public class UpdateProductsCommand : IRequest<bool>
{
    public List<UpdateProductRequest> Products { get; set; }
}
