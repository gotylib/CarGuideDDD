using CarGuideDDD.MailService.Services.Interfaces;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CarGuideDDD.MailService.Services.Producers
{
    public sealed class ProducerHostedService : IDisposable, IProducerHostedService
    {
        private readonly ILogger<ProducerHostedService> _logger;


        public ProducerHostedService(ILogger<ProducerHostedService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose() { }


        public void SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message);
        }

        public async void SendMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672 };

            using var connection = await factory.CreateConnectionAsync();
            using (var channel = await connection.CreateChannelAsync())
            {
                await channel.QueueDeclareAsync(queue: "MailMessages.DLM",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(exchange: "",
                    routingKey: "MailMessages.DLM",
                    mandatory: false,
                    basicProperties: new BasicProperties(),
                    body: body);
            }

        }
    }
}
