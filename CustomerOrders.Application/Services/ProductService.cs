using CustomerOrders.Application.Helpers.Mappers;
using CustomerOrders.Application.Interfaces;
using CustomerOrders.Application.Services.RabbitMQ;
using CustomerOrders.Core.Entities;
using CustomerOrders.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerOrders.Application.Services
{
    /// <summary>
    /// Handles product-related operations, including caching and RabbitMQ notifications.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repository;
        private readonly RedisCacheService _redisCacheService;
        private readonly ILogger<ProductService> _logger;
        private readonly RabbitMqService _rabbitMqService;

        public ProductService(
            IRepository<Product> repository,
            RedisCacheService redisCacheService,
            ILogger<ProductService> logger,
            RabbitMqService rabbitMqService)
        {
            _repository = repository;
            _redisCacheService = redisCacheService;
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        /// <summary>
        /// Retrieves all products, checking Redis cache first. Caches DB data if no cache found.
        /// </summary>
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var cachedProducts = await _redisCacheService.GetProductListFromCacheAsync();
                if (cachedProducts != null)
                {
                    _logger.LogInformation("Products retrieved from Redis cache.");
                    return cachedProducts.Select(p => p.ToDto());
                }

                var products = await _repository.Query().ToListAsync();
                await _redisCacheService.CacheProductListAsync(products);
                _logger.LogInformation("Products retrieved from DB and cached.");

                return products.Select(p => p.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting the product list.");
                return Enumerable.Empty<ProductDto>();
            }
        }

        /// <summary>
        /// Gets details of a single product by its unique ID.
        /// </summary>
        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _repository.Query().FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found.", id);
                    return null;
                }

                return product.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID {ProductId}", id);
                return null;
            }
        }

        /// <summary>
        /// Adds a new product to the database, updates Redis cache, and notifies RabbitMQ.
        /// </summary>
        /// <param name="product">The product entity to add.</param>
        public async Task AddProductAsync(Product product)
        {
            try
            {
                await _repository.AddAsync(product);
                var products = await _repository.Query().ToListAsync();
                await _redisCacheService.CacheProductListAsync(products);

                var message = $"[ADD] Product added: {product.Description} (ID: {product.Id}, Price: {product.Price})";
                _rabbitMqService.SendMessage(message);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new product.");
            }
        }

        /// <summary>
        /// Updates an existing product, refreshes Redis cache, and notifies RabbitMQ.
        /// </summary>
        /// <param name="product">The product entity with new data.</param>
        public async Task UpdateProductAsync(Product product)
        {
            try
            {
                await _repository.UpdateAsync(product);
                var products = await _repository.Query().ToListAsync();
                await _redisCacheService.CacheProductListAsync(products);

                var message = $"[UPDATE] Product updated: {product.Description} (ID: {product.Id}, Price: {product.Price})";
                _rabbitMqService.SendMessage(message);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", product.Id);
            }
        }

        /// <summary>
        /// Removes a product by ID, updates Redis cache, and sends a RabbitMQ notification.
        /// </summary>
        /// <param name="id">The product ID to delete.</param>
        public async Task DeleteProductAsync(int id)
        {
            try
            {
                var product = await _repository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Delete failed. Product with ID {ProductId} not found.", id);
                    return;
                }

                await _repository.DeleteAsync(product);
                var products = await _repository.Query().ToListAsync();
                await _redisCacheService.CacheProductListAsync(products);

                var message = $"[DELETE] Product deleted: {product.Description} (ID: {product.Id})";
                _rabbitMqService.SendMessage(message);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
            }
        }
    }
}
