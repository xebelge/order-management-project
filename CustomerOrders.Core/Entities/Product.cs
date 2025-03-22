namespace CustomerOrders.Core.Entities
{
    /// <summary>
    /// Holds product related informations.
    /// </summary>
    public class Product : BaseEntity
    {
        public string Barcode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public List<CustomerOrderProduct> CustomerOrderProducts { get; set; } = new();
    }
}
