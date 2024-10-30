using CarGuideDDD.MailService.Objects;
using CarGuideDDD.MailService.Services.Interfaces;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace CarGuideDDD.MailService.Services
{
    public enum TypeOfMessage
    {
        SendErrorMessageNoHaveCar,
        SendErrorMessageNoHaveManagers,
        SendInfoMessage,
        SendBuyMessage
    }
    
    
    public sealed class ConsumerHostedService : BackgroundService
    {
        private readonly IConsumer<int, string> _consumer;
        private readonly string _topic;
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IMailServices _mailServices;

        public ConsumerHostedService(IConsumer<int, string> consumer, string topic,
            ILogger<ConsumerHostedService> logger, IMailServices mailServices)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailServices = mailServices;
        }
        

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumer starting...");
            var consumeLoop = Task.Run(() => Consume(cancellationToken), cancellationToken);
            _logger.LogInformation("Consumer started.");
            return consumeLoop;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumer stopping...");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Consumer stopped.");
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();

            base.Dispose();
        }

        private void Consume(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Message consuming");
                    var result = _consumer.Consume(cancellationToken);
                    var jsonmessage = result.Message.Value;
                    var type = result.Message.Key;
                    var responce = JsonConvert.DeserializeObject<MailSendObj>(jsonmessage);
                    if (responce == null) throw OperationCanceledException(); 
                    switch (type)
                        {
                            case (int)TypeOfMessage.SendErrorMessageNoHaveCar:
                                _mailServices.SendUserNoHaveCarMessage(responce.User, responce.Car);
                                _consumer.Commit(result);
                                break;
                            case (int)TypeOfMessage.SendErrorMessageNoHaveManagers:
                                _mailServices.SendUserNotFountManagerMessage(responce.User);
                                _consumer.Commit(result);
                                break;
                            case (int)TypeOfMessage.SendBuyMessage:
                                _mailServices.SendBuyCarMessage(responce.User, responce.Manager, responce.Car);
                                _consumer.Commit(result);
                                break;
                            case (int)TypeOfMessage.SendInfoMessage:
                                _mailServices.SendInformCarMessage(responce.User, responce.Manager, responce.Car);
                                _consumer.Commit(result);
                                break;
                            default:
                                _logger.LogInformation("Message dont convert to object.", result.Message.Value);
                                break;
                        }
                        
                        _logger.LogInformation("Message '{Message}' consumed.", result.Message.Value);

                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Message consuming failed.");

                    if (ex.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Message consuming failed with unexpected error.");
                }
            }
        }
    }
}
