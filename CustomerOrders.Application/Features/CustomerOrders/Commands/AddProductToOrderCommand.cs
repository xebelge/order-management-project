using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Commands
{
    public class AddProductToOrderCommand : IRequest<bool>
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
