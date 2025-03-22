/// <summary>
/// Holds updated customer info such as name, address, and email change requests.
/// </summary>
public class UpdateCustomerInfoRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? OldEmail { get; set; }
    public string? NewEmail { get; set; }
}
