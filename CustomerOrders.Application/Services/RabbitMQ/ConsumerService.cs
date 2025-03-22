using System.Text;
using CustomerOrders.Application.Services.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CustomerOrders.Application.Services.ConsumerService
{
    public class ConsumerService
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly string _filePath = "notifications.txt";

        public ConsumerService(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_rabbitMqService.Channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"Received: {message}");

                WriteToFile(message);
            };

            _rabbitMqService.Channel.BasicConsume(queue: "order_notifications", autoAck: true, consumer: consumer);
        }

        private void WriteToFile(string message)
        {
            using (var writer = new StreamWriter(_filePath, append: true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
