using MediatR;

namespace CustomerOrders.Application.Features.Products.Command
{
    public class AddProductsCommand : IRequest<bool>
    {
        public List<CreateProductRequest> Products { get; set; }
    }
}
