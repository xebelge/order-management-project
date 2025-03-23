using MediatR;

namespace CustomerOrders.Application.Features.CustomerOrders.Commands
{
    public class DeleteCustomerOrderCommand : IRequest<bool>
    {
        public int OrderId { get; set; }
    }

}
