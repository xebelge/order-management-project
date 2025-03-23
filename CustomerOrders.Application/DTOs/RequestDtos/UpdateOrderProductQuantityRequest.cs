namespace CustomerOrders.Application.DTOs.RequestDtos
{
    public class UpdateOrderProductQuantityRequest
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
