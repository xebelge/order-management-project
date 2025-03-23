using CustomerOrders.Core.Entities;

///
/// Represents the link between a customer order and a product including quantity details.
///
public class CustomerOrderProduct: BaseEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int ProductQuantity { get; set; }
    public int CustomerOrderId { get; set; }
}
