using System.Text;
using CustomerOrders.Application.Services.RabbitMQ;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CustomerOrders.Application.Services.ConsumerService
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue and writes them to a local file.
    /// </summary>
    public class ConsumerService
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<ConsumerService> _logger;
        private readonly string _filePath = "notifications.txt";

        public ConsumerService(RabbitMqService rabbitMqService, ILogger<ConsumerService> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public void StartConsuming()
        {
            _logger.LogInformation("RabbitMQ consumer started. Queue: order_notifications");

            var consumer = new EventingBasicConsumer(_rabbitMqService.Channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogInformation("New message received. Message length: {Length}", message.Length);

                try
                {
                    WriteToFile(message);
                    _logger.LogDebug("Message successfully written to file.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while writing the message to the file.");
                }
            };

            _rabbitMqService.Channel.BasicConsume(
                queue: "order_notifications",
                autoAck: true,
                consumer: consumer);
        }

        /// <summary>
        /// Appends the message content into a local text file with timestamp.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void WriteToFile(string message)
        {
            using var writer = new StreamWriter(_filePath, append: true);
            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}");
        }
    }
}
