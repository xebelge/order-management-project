/// <summary>
/// Represents a customer with basic profile and deletion status info.
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
