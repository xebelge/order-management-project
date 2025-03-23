/// <summary>
/// Encapsulates the current and new password for a customer seeking to change passwords.
/// </summary>
namespace CustomerOrders.Application.DTOs.RequestDtos
{
    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
