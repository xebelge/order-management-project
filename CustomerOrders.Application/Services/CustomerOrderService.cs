using CustomerOrders.Application.Helpers.Mappers;
using CustomerOrders.Application.Services.RabbitMQ;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerOrders.Application.Services
{
    /// <summary>
    /// Provides operations to handle customer orders and related notifications.
    /// </summary>
    public class CustomerOrderService
    {
        private readonly IRepository<CustomerOrder> _repository;
        private readonly IRepository<Product> _productRepository;
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<CustomerOrderService> _logger;

        public CustomerOrderService(
            IRepository<CustomerOrder> repository,
            IRepository<Product> productRepository,
            RabbitMqService rabbitMqService,
            ILogger<CustomerOrderService> logger)
        {
            _repository = repository;
            _productRepository = productRepository;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all customer orders, including product details.
        /// </summary>
        public async Task<IEnumerable<CustomerOrderDto>> GetAllOrdersAsync()
        {
            var orders = await _repository.Query()
                .Include(o => o.CustomerOrderProducts)
                .ThenInclude(cop => cop.Product)
                .ToListAsync();

            _logger.LogInformation("All orders have been brought in. Total: {Count}", orders.Count);

            return orders.Select(o => o.ToDto());
        }

        /// <summary>
        /// Retrieves a single order DTO by its ID.
        /// </summary>
        /// <param name="id">Order ID.</param>
        public async Task<CustomerOrderDto> GetOrderDtoById(int id)
        {
            var order = await _repository.Query()
                .Include(o => o.CustomerOrderProducts)
                .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order not found. ID: {OrderId}", id);
                return null;
            }

            _logger.LogInformation("Order found. ID: {OrderId}", id);
            return order.ToDto();
        }

        /// <summary>
        /// Gets a raw CustomerOrder entity by its ID, including product details.
        /// </summary>
        /// <param name="id">Order ID.</param>
        public async Task<CustomerOrder> GetOrderByIdAsync(int id)
        {
            return await _repository.Query()
                .Include(o => o.CustomerOrderProducts)
                .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        /// <summary>
        /// Adds a new order to the system, checking product stock and sending a notification.
        /// </summary>
        /// <param name="order">The order entity to add.</param>
        public async Task AddOrderAsync(CustomerOrder order)
        {
            foreach (var cop in order.CustomerOrderProducts)
            {
                var product = await _productRepository.GetByIdAsync(cop.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Order not found. ID: {ProductId}", cop.ProductId);
                    throw new InvalidOperationException($"Product with ID {cop.ProductId} not found.");
                }

                if (product.Quantity < 1)
                {
                    _logger.LogWarning("Insufficient stock: {Description} (ID: {ProductId})", product.Description, product.Id);
                    throw new InvalidOperationException($"Insufficient stock: {product.Description}");
                }
            }

            await _repository.AddAsync(order);

            var message = $"Order added: CustomerId: {order.CustomerId}, OrderId: {order.Id}";
            _rabbitMqService.SendMessage(message);
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Deletes an existing order if found, and sends a notification.
        /// </summary>
        /// <param name="id">Order ID to delete.</param>
        public async Task DeleteOrderAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order != null)
            {
                await _repository.DeleteAsync(order);

                var message = $"Order deleted: OrderId: {order.Id}, CustomerId: {order.CustomerId}";
                _rabbitMqService.SendMessage(message);
                _logger.LogInformation(message);
            }
            else
            {
                _logger.LogWarning("The order to be deleted could not be found. ID: {OrderId}", id);
            }
        }

        /// <summary>
        /// Updates an existing order in the database and sends a notification.
        /// </summary>
        /// <param name="order">The order entity with changes.</param>
        public async Task UpdateOrderAsync(CustomerOrder order)
        {
            await _repository.UpdateAsync(order);

            var message = $"Order updated: OrderId: {order.Id}, CustomerId: {order.CustomerId}";
            _rabbitMqService.SendMessage(message);
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Retrieves a product entity by its ID through the internal product repository.
        /// </summary>
        /// <param name="productId">Product ID.</param>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }
    }
}