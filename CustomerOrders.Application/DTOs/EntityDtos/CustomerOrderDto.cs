/// <summary>
/// Represents a customer's order, including total cost and product details.
/// </summary>
public class CustomerOrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<CustomerOrderProductDto> Products { get; set; } = new();
}