using CustomerOrders.Application.Features.CustomerOrders.Commands;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Features.CustomerOrders.Handlers
{
    public class UpdateProductQuantityInOrderCommandHandler : IRequestHandler<UpdateProductQuantityInOrderCommand, bool>
    {
        private readonly IRepository<CustomerOrder> _orderRepository;
        private readonly IRepository<Product> _productRepository;

        public UpdateProductQuantityInOrderCommandHandler(
            IRepository<CustomerOrder> orderRepository,
            IRepository<Product> productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(UpdateProductQuantityInOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.CustomerOrderProducts)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return false;

            var productInOrder = order.CustomerOrderProducts.FirstOrDefault(p => p.ProductId == request.ProductId);
            if (productInOrder == null)
                return false;

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null || request.Quantity > product.Quantity)
                return false;

            productInOrder.ProductQuantity = request.Quantity;
            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}
