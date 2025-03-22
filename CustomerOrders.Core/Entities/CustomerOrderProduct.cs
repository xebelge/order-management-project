using CustomerOrders.Core.Entities;

public class CustomerOrderProduct: BaseEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int ProductQuantity { get; set; }
}
