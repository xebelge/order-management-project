using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Commands
{
    public class UpdateProductQuantityInOrderCommand : IRequest<bool>
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
