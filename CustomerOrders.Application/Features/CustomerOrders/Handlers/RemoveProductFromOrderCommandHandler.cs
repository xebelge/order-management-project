using CustomerOrders.Application.Features.CustomerOrders.Commands;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Features.CustomerOrders.Handlers
{
    public class RemoveProductFromOrderCommandHandler : IRequestHandler<RemoveProductFromOrderCommand, bool>
    {
        private readonly IRepository<CustomerOrder> _orderRepository;

        public RemoveProductFromOrderCommandHandler(IRepository<CustomerOrder> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> Handle(RemoveProductFromOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.CustomerOrderProducts)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return false;

            var product = order.CustomerOrderProducts.FirstOrDefault(p => p.ProductId == request.ProductId);
            if (product == null)
                return false;

            order.CustomerOrderProducts.Remove(product);
            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}
