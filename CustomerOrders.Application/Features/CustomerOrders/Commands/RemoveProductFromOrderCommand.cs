using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Commands
{
    public class RemoveProductFromOrderCommand : IRequest<bool>
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
    }
}
