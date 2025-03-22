/// <summary>
/// Defines a new customer order with delivery address and related products.
/// </summary>
public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public string Address { get; set; }
    public List<AddOrderProductItemRequest> ProductItems { get; set; } = new();
}