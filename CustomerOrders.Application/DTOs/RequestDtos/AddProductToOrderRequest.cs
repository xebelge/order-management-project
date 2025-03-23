/// <summary>
/// Contains the product details needed to add an item to an existing order.
/// </summary>
public class AddProductToOrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
