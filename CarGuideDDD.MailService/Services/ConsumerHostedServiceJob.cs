using CarGuideDDD.MailService.Objects;
using CarGuideDDD.MailService.Services.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace CarGuideDDD.MailService.Services
{
    public class ConsumerHostedServiceJob : BackgroundService
    {
        private IConnection _connection;
        private IChannel _channel;
        private IMailServices _mailServices;
        private ILogger<ConsumerHostedService> _logger;
        private IProducerHostedService _producerHostedService;

        public ConsumerHostedServiceJob(IMailServices mailServices, ILogger<ConsumerHostedService> logger, IProducerHostedService producerHostedService)
        {
            _mailServices = mailServices;
            _logger = logger;
            _producerHostedService = producerHostedService;
        }

        public static async Task<ConnectToRabbitMq> CreateAsync()
        {
            var factory = new ConnectionFactory { HostName = "rabbitmq", Port = 5672 };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: "MailSendJob",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            return new ConnectToRabbitMq(connection, channel);
        }                    

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var connect = await CreateAsync();

            _connection = connect.Connection;
            _channel = connect.Channel;

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    var responce = JsonConvert.DeserializeObject<List<string?>>(content);
                    if (responce != null)
                    {
                        _mailServices.SendReminderToAddCar(responce);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Message '{Message}' consumed.", content);
                    }


                }
                catch (Exception ex)
                {
                    _logger.LogError("Massage dont processed", ex);
                    _producerHostedService.SendMessage(content, "MailSendJob.DLM");

                }
            };

            await _channel.BasicConsumeAsync("MailSendJob", false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async void Dispose()
        {
            base.Dispose();
        }
    }
}
