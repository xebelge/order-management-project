using CustomerOrders.Application.Features.CustomerOrders.Commands;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Features.CustomerOrders.Handlers
{
    public class AddProductToOrderCommandHandler : IRequestHandler<AddProductToOrderCommand, bool>
    {
        private readonly IRepository<CustomerOrder> _orderRepository;
        private readonly IRepository<Product> _productRepository;

        public AddProductToOrderCommandHandler(
            IRepository<CustomerOrder> orderRepository,
            IRepository<Product> productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(AddProductToOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.CustomerOrderProducts)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return false;

            if (order.CustomerOrderProducts.Any(p => p.ProductId == request.ProductId))
                return false;

            order.CustomerOrderProducts.Add(new CustomerOrderProduct
            {
                ProductId = request.ProductId,
                ProductQuantity = request.Quantity
            });

            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}
