/// <summary>
/// Represents a product within a customer's order, including quantity and pricing.
/// </summary>
public class CustomerOrderProductDto
{
    public int ProductId { get; set; }
    public int ProductQuantity { get; set; }
    public string Barcode { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}
