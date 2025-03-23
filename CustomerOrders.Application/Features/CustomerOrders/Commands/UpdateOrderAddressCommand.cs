using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Commands
{
    public class UpdateOrderAddressCommand : IRequest<bool>
    {
        public int OrderId { get; set; }
        public string Address { get; set; } = string.Empty;
    }

}