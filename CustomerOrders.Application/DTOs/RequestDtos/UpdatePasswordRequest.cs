namespace CustomerOrders.Application.DTOs.RequestDtos
{
    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
