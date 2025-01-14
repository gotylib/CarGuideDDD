using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using CarGuideDDD.MailService.Objects;
using CarGuideDDD.MailService.Services.Interfaces;

namespace CarGuideDDD.MailService.Services
{
    public enum TypeOfMessage
    {
        SendErrorMessageNoHaveCar,
        SendErrorMessageNoHaveManagers,
        SendInfoMessage,
        SendBuyMessage,
        SendReminderToAddCar
    }


    public class ConsumerHostedService : BackgroundService
    {
        private IConnection _connection;
        private IChannel _channel;
        private IMailServices _mailServices;
        private ILogger<ConsumerHostedService> _logger;
        private IProducerHostedService _producerHostedService;

        public ConsumerHostedService(IMailServices mailServices, ILogger<ConsumerHostedService> logger, IProducerHostedService producerHostedService)
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
            await channel.QueueDeclareAsync(queue: "MailMessages",
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
                    var responce = JsonConvert.DeserializeObject<MailSendObj>(content);
                    if (responce != null)
                    {
                        var type = responce.EventId;
                        switch (type)
                        {
                            case (int)TypeOfMessage.SendErrorMessageNoHaveCar:
                                _mailServices.SendUserNoHaveCarMessage(responce.User, responce.Car);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                _logger.LogInformation("Message '{Message}' consumed.", content);
                                break;
                            case (int)TypeOfMessage.SendErrorMessageNoHaveManagers:
                                _mailServices.SendUserNotFountManagerMessage(responce.User);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                _logger.LogInformation("Message '{Message}' consumed.", content);
                                break;
                            case (int)TypeOfMessage.SendBuyMessage:
                                _mailServices.SendBuyCarMessage(responce.User, responce.Manager, responce.Car);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                _logger.LogInformation("Message '{Message}' consumed.", content);
                                break;
                            case (int)TypeOfMessage.SendInfoMessage:
                                _mailServices.SendInformCarMessage(responce.User, responce.Manager, responce.Car);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                _logger.LogInformation("Message '{Message}' consumed.", content);
                                break;
                            case (int)TypeOfMessage.SendReminderToAddCar:
                                _mailServices.SendReminderToAddCar(responce.User);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                _logger.LogInformation("Message '{Message}' consumed.", content);
                                break;
                            default:
                                _logger.LogInformation("Message dont convert to object.", content);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                _producerHostedService.SendMessage(content);
                                break;
                        }
                    }
                    
                    
                }
                catch(Exception ex)
                {
                    _logger.LogError("Massage dont processed", ex);
                    _producerHostedService.SendMessage(content);

                }
            };

            await _channel.BasicConsumeAsync("MailMessages", false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async void Dispose()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
            base.Dispose();
        }
    }
}
