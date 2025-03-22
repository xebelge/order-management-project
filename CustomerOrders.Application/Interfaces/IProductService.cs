using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Interfaces
{
    /// <summary>
    /// Defines the operations for managing products in the system.
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
