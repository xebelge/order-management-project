/// <summary>
/// Standardized API response wrapper containing status, message, and optional payload.
/// </summary>
/// <typeparam name="T">Type of the data payload.</typeparam>

namespace CustomerOrders.Application.DTOs
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponseDto(int statusCode, bool success, string message, T data = default)
        {
            StatusCode = statusCode;
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
