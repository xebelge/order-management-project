using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Services
{
    public class CustomerOrderService
    {
        private readonly IRepository<CustomerOrder> _repository;
        private readonly IRepository<Product> _productRepository;

        public CustomerOrderService(IRepository<CustomerOrder> repository, IRepository<Product> productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<CustomerOrderDto>> GetAllOrdersAsync()
        {
            var orders = await _repository.Query()
                .Include(o => o.CustomerOrderProducts)
                .ThenInclude(cop => cop.Product)
                .ToListAsync();

            return orders.Select(order => new CustomerOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Address = order.Address,
                TotalAmount = order.CustomerOrderProducts.Sum(cop => cop.Product.Price * cop.ProductQuantity),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Products = order.CustomerOrderProducts.Select(cop => new CustomerOrderProductDto
                {
                    ProductId = cop.ProductId,
                    ProductQuantity = cop.ProductQuantity,
                }).ToList()
            });
        }

        public async Task<CustomerOrderDto> GetOrderDtoById(int id)
        {
            var order = await _repository.Query()
                .Include(o => o.CustomerOrderProducts)
                .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return null;

            return new CustomerOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Address = order.Address,
                TotalAmount = order.CustomerOrderProducts.Sum(cop => cop.Product.Price),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Products = order.CustomerOrderProducts.Select(cop => new CustomerOrderProductDto
                {
                    ProductId = cop.Product.Id,
                    ProductQuantity = cop.ProductQuantity,
                    Barcode = cop.Product.Barcode,
                    Description = cop.Product.Description,
                    Price = cop.Product.Price
                }).ToList()
            };
        }

        public async Task<CustomerOrder> GetOrderByIdAsync(int id)
        {
            return await _repository.Query()
                .Include(o => o.CustomerOrderProducts)
                .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task AddOrderAsync(CustomerOrder order)
        {
            foreach (var cop in order.CustomerOrderProducts)
            {
                var product = await _productRepository.GetByIdAsync(cop.ProductId);

                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {cop.ProductId} not found.");
                }

                if (product.Quantity < 1)
                {
                    throw new InvalidOperationException($"Insufficient stock: {product.Description}. Available: {product.Quantity}, Requested: 1");
                }
            }

            await _repository.AddAsync(order);
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order != null)
            {
                await _repository.DeleteAsync(order);
            }
        }

        public async Task UpdateOrderAsync(CustomerOrder order)
        {
            await _repository.UpdateAsync(order);
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

    }
}
