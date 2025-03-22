using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerOrders.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repository;
        private readonly RedisCacheService _redisCacheService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IRepository<Product> repository, RedisCacheService redisCacheService, ILogger<ProductService> logger)
        {
            _repository = repository;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Attempting to retrieve products from cache.");
            var cachedProducts = await _redisCacheService.GetProductListFromCacheAsync();
            if (cachedProducts != null)
            {
                _logger.LogInformation("Products found in cache. Returning cached products.");
                return cachedProducts.Select(product => new ProductDto
                {
                    Id = product.Id,
                    Barcode = product.Barcode,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    Price = product.Price
                });
            }

            _logger.LogInformation("Cache miss. Fetching products from database.");
            var products = await _repository.Query().ToListAsync();

            _logger.LogInformation("Caching product list.");
            await _redisCacheService.CacheProductListAsync(products);

            return products.Select(product => new ProductDto
            {
                Id = product.Id,
                Barcode = product.Barcode,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price
            });
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving product by ID: {ProductId}", id);
            var product = await _repository.Query().FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
                return null;
            }

            _logger.LogInformation("Product with ID {ProductId} found. Returning product details.", id);

            return new ProductDto
            {
                Id = product.Id,
                Barcode = product.Barcode,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price
            };
        }

        public async Task AddProductAsync(Product product)
        {
            _logger.LogInformation("Adding new product with ID {ProductId}", product.Id);
            await _repository.AddAsync(product);

            _logger.LogInformation("Fetching all products after adding new one.");
            var products = await _repository.Query().ToListAsync();

            _logger.LogInformation("Caching updated product list after adding new product.");
            await _redisCacheService.CacheProductListAsync(products);
        }

        public async Task UpdateProductAsync(Product product)
        {
            _logger.LogInformation("Updating product with ID {ProductId}", product.Id);
            await _repository.UpdateAsync(product);

            _logger.LogInformation("Fetching all products after update.");
            var products = await _repository.Query().ToListAsync();

            _logger.LogInformation("Caching updated product list after updating product.");
            await _redisCacheService.CacheProductListAsync(products);
        }

        public async Task DeleteProductAsync(int id)
        {
            _logger.LogInformation("Deleting product with ID {ProductId}", id);
            var product = await _repository.GetByIdAsync(id);
            if (product != null)
            {
                _logger.LogInformation("Product found. Deleting product with ID {ProductId}", id);
                await _repository.DeleteAsync(product);

                _logger.LogInformation("Fetching all products after deletion.");
                var products = await _repository.Query().ToListAsync();

                _logger.LogInformation("Caching updated product list after deletion.");
                await _redisCacheService.CacheProductListAsync(products);
            }
            else
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion.", id);
            }
        }
    }
}
