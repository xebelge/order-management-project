namespace CustomerOrders.Application.DTOs
{
    /// <summary>
    /// Provides the data required to update an existing product.
    /// </summary>
    public class UpdateProductRequest
    {
        public int Id { get; set; }                   
        public string Barcode { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public decimal Price { get; set; }             
        public int Quantity { get; set; }           
    }
}
