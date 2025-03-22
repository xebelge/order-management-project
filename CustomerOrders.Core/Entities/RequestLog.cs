namespace CustomerOrders.Core.Entities
{
    /// <summary>
    /// Stores details of an API request, including paths, bodies, and status code.
    /// </summary>
    public class RequestLog
    {
        public int Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
