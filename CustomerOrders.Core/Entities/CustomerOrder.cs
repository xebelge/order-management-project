namespace CustomerOrders.Core.Entities
{
    public class CustomerOrder : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public string Address { get; set; }
        public List<CustomerOrderProduct> CustomerOrderProducts { get; set; } = new();
        public decimal TotalAmount => CustomerOrderProducts?.Sum(cop => cop.Product.Price) ?? 0;
    }
}
