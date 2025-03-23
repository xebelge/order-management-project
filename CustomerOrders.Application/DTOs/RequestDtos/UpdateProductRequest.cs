/// <summary>
/// Provides the data required to update an existing product.
/// </summary>
namespace CustomerOrders.Application.DTOs
{
    public class UpdateProductRequest
    {
        public int Id { get; set; }                   
        public string Barcode { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
}
