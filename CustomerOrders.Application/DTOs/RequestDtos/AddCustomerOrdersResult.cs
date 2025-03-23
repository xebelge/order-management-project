namespace CustomerOrders.Application.DTOs.RequestDtos
{
    public class AddCustomerOrdersResult
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public IEnumerable<CustomerOrderDto>? Orders { get; private set; }

        public static AddCustomerOrdersResult SuccessResult(IEnumerable<CustomerOrderDto> orders)
        {
            return new AddCustomerOrdersResult { Success = true, Orders = orders };
        }

        public static AddCustomerOrdersResult Fail(string errorMessage)
        {
            return new AddCustomerOrdersResult { Success = false, ErrorMessage = errorMessage };
        }
    }

}
