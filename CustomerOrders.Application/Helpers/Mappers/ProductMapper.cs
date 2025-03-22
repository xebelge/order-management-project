using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Helpers.Mappers
{
    /// <summary>
    /// Converts a Product entity into a DTO format for API use.
    /// </summary>
    public static class ProductMapper
    {
        public static ProductDto ToDto(this Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Description = p.Description,
                Quantity = p.Quantity,
                Price = p.Price
            };
        }
    }
}
