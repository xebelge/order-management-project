using System.Text;
using CustomerOrders.Core.Entities;
using CustomerOrders.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CustomerOrders.Application.Middleware
{
    /// <summary>
    /// Logs incoming requests and outgoing responses storing them in the RequestLogs table.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AppDbContext dbContext)
        {
            var request = context.Request;
            var originalResponseBodyStream = context.Response.Body;

            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            string requestBody = string.Empty;

            try
            {
                if (request.ContentLength > 0 && request.Body.CanRead)
                {
                    request.EnableBuffering();
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RequestBody Error] {ex.Message}");
            }

            await _next(context);

            string responseBody = string.Empty;
            try
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ResponseBody Error] {ex.Message}");
            }

            var log = new RequestLog
            {
                Path = request.Path,
                Method = request.Method,
                Username = context.User.Identity?.IsAuthenticated == true ? context.User.Identity?.Name : null,
                RequestBody = FormatForDb(requestBody),
                ResponseBody = FormatForDb(responseBody),
                StatusCode = context.Response.StatusCode,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                dbContext.RequestLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logging DB Error] {ex.Message}");
            }

            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }

        /// <summary>
        /// Removes escape characters and flattens JSON otherwise returns raw text.
        /// </summary>
        private string FormatForDb(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "";

            try
            {
                var token = JsonConvert.DeserializeObject(body);
                return JsonConvert.SerializeObject(token, Formatting.None);
            }
            catch
            {
                return body;
            }
        }
    }
}
