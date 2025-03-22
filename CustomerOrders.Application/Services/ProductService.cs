using CustomerOrders.Application.Interfaces;
using CustomerOrders.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerOrders.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repository;

        public ProductService(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repository.Query()
                .ToListAsync();

            return products.Select(product => new ProductDto
            {
                Id = product.Id,
                Barcode = product.Barcode,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price,
            });
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _repository.Query()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Barcode = product.Barcode,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price,
            };
        }

        public async Task AddProductAsync(Product product) => await _repository.AddAsync(product);

        public async Task UpdateProductAsync(Product product) => await _repository.UpdateAsync(product);

        public async Task DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is not null)
                await _repository.DeleteAsync(product);
        }
    }
}
