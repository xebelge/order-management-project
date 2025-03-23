namespace CustomerOrders.Application.DTOs.RequestDtos
{
    /// <summary>
    /// Holds customer credentials needed for registration.
    /// </summary>
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Represents a customer's login credentials.
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
