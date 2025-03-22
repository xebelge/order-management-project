/// <summary>
/// Holds the product ID and quantity for an product being added to an order.
/// </summary>
public class AddOrderProductItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}