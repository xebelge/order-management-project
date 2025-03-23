using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CustomerOrders.Application.Services.RabbitMQ
{
    /// <summary>
    /// Handles publishing messages to RabbitMQ.
    /// </summary>
    public class RabbitMqService : IDisposable
    {
        private readonly string _hostName;
        private readonly string _queueName;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly ILogger<RabbitMqService> _logger;

        private IConnection _connection;
        private IModel _channel;
        private bool _disposed;

        public IModel Channel => _channel;

        public RabbitMqService(IConfiguration configuration, ILogger<RabbitMqService> logger)
        {
            _logger = logger;

            var rabbitMqSettings = configuration.GetSection("RabbitMQ");

            _hostName = rabbitMqSettings["Host"] ?? "localhost";
            _queueName = rabbitMqSettings["QueueName"] ?? "customer_order_notifications";
            _port = int.TryParse(rabbitMqSettings["Port"], out var port) ? port : 5672;
            _username = rabbitMqSettings["Username"] ?? "guest";
            _password = rabbitMqSettings["Password"] ?? "guest";

            InitializeConnection();
        }

        private void InitializeConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Port = _port,
                    UserName = _username,
                    Password = _password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _logger.LogInformation("RabbitMQ connection and channel successfully created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection.");
                throw;
            }
        }

        public void SendMessage(string message)
        {
            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning("RabbitMQ channel null or closed. Reinitializing connection.");
                InitializeConnection();
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Attempted to send empty message.");
                return;
            }

            try
            {
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: _queueName,
                    basicProperties: null,
                    body: body
                );

                _logger.LogInformation("Message sent to queue '{QueueName}': {Message}", _queueName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to RabbitMQ.");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection and channel disposed.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred while disposing RabbitMQ resources.");
            }

            _disposed = true;
        }
    }
}
