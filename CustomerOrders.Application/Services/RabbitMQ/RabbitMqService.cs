using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace CustomerOrders.Application.Services.RabbitMQ
{
    public class RabbitMqService
    {
        private readonly string _hostName;
        private readonly string _queueName;
        private IConnection _connection;
        private IModel _channel;

        public IModel Channel => _channel;


        public RabbitMqService(IConfiguration configuration)
        {
            var rabbitMqSettings = configuration.GetSection("RabbitMQ");
            _hostName = rabbitMqSettings["Host"] ?? "localhost";
            _queueName = rabbitMqSettings["QueueName"] ?? "order_notifications";
            var port = rabbitMqSettings["Port"] ?? "5672";

            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                Port = int.Parse(port),
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                                  routingKey: _queueName,
                                  basicProperties: null,
                                  body: body);
        }
    }
}
