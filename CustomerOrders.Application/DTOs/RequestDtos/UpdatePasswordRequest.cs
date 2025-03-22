/// <summary>
/// Encapsulates the current and new password for a user seeking to change passwords.
/// </summary>
namespace CustomerOrders.Application.DTOs.RequestDtos
{
    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
