/// <summary>
/// Holds the essential product data required to create a new product.
/// </summary>
public class CreateProductRequest
{
    public string Barcode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
