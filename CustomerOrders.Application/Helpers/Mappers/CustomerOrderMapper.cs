using CustomerOrders.Core.Entities;

namespace CustomerOrders.Application.Helpers.Mappers
{
    /// <summary>
    /// Converts a CustomerOrder entity into a DTO including its related products.
    /// </summary>
    public static class CustomerOrderMapper
    {
        public static CustomerOrderDto ToDto(this CustomerOrder order)
        {
            return new CustomerOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Address = order.Address,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                TotalAmount = order.CustomerOrderProducts?.Sum(cop => cop.Product.Price) ?? 0,
                Products = order.CustomerOrderProducts?.Select(cop => new CustomerOrderProductDto
                {
                    ProductId = cop.ProductId,
                    ProductQuantity = cop.ProductQuantity,
                    Barcode = cop.Product.Barcode,
                    Description = cop.Product.Description,
                    Price = cop.Product.Price
                }).ToList() ?? new()
            };
        }
    }
}
