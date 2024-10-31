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
    
    
    public sealed class ConsumerHostedService(
        IConsumer<int, string> consumer,
        string topic,
        ILogger<ConsumerHostedService> logger,
        IMailServices mailServices,
        KafkaMessageProducer kafkaMessageProducer,
        ILogger<ProducerHostedService> loggerProduce)
        : BackgroundService
    {
        private readonly IConsumer<int, string> _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        private readonly ILogger<ConsumerHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly KafkaMessageProducer _kafkaMessageProducer = kafkaMessageProducer ?? throw new ArgumentNullException(nameof(kafkaMessageProducer));
        private readonly ILogger<ProducerHostedService> _loggerProducer = loggerProduce ?? throw new ArgumentNullException(nameof(loggerProduce));


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
                    if (responce == null) throw new OperationCanceledException(); 
                    switch (type)
                        {
                            case (int)TypeOfMessage.SendErrorMessageNoHaveCar:
                                mailServices.SendUserNoHaveCarMessage(responce.User, responce.Car);
                                _consumer.Commit(result);
                                _logger.LogInformation("Message '{Message}' consumed.", result.Message.Value);
                                break;
                            case (int)TypeOfMessage.SendErrorMessageNoHaveManagers:
                                mailServices.SendUserNotFountManagerMessage(responce.User);
                                _consumer.Commit(result);
                                _logger.LogInformation("Message '{Message}' consumed.", result.Message.Value);
                                break;
                            case (int)TypeOfMessage.SendBuyMessage:
                                mailServices.SendBuyCarMessage(responce.User, responce.Manager, responce.Car);
                                _consumer.Commit(result);
                                _logger.LogInformation("Message '{Message}' consumed.", result.Message.Value);
                                break;
                            case (int)TypeOfMessage.SendInfoMessage:
                                mailServices.SendInformCarMessage(responce.User, responce.Manager, responce.Car);
                                _consumer.Commit(result);
                                _logger.LogInformation("Message '{Message}' consumed.", result.Message.Value);
                                break;
                            default:
                                _logger.LogInformation("Message dont convert to object.", result.Message.Value);
                                var producer = new ProducerHostedService(_kafkaMessageProducer, loggerProduce);
                                producer.SendMessage(result.Message.Key, result.Message.Value);
                                _consumer.Commit(result);
                                break;
                        }

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
