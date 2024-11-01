using Confluent.Kafka;
using Newtonsoft.Json;
using CarGuideDDD.MailService.Objects;
using CarGuideDDD.MailService.Services.Producers;
using CarGuideDDD.MailService.Services.Interfaces;

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
        RederectMessageProducer rederectMessageProducer,
        KafkaAdmin.KafkaAdmin kafkaAdmin)
        : BackgroundService
    {
        private readonly IConsumer<int, string> _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        private readonly ILogger<ConsumerHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly KafkaMessageProducer _kafkaMessageProducer = kafkaMessageProducer ?? throw new ArgumentNullException(nameof(kafkaMessageProducer));
        private readonly RederectMessageProducer _rederectMessageProducer =
            rederectMessageProducer ?? throw new ArgumentNullException(nameof(rederectMessageProducer));

        private readonly KafkaAdmin.KafkaAdmin _kafkaAdmin = kafkaAdmin ?? throw new ArgumentNullException(nameof(kafkaAdmin));

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

        private async Task Consume(CancellationToken cancellationToken)
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
                        
                    try
                    {
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
                                await _kafkaMessageProducer.Message(result.Message.Key, result.Message.Value);
                                _consumer.Commit(result);
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        //тут можно установить вместо 2 колличество партиций у нас их две.
                        if (responce.Score == 2)
                        {
                            await _kafkaMessageProducer.Message(result.Message.Key, result.Message.Value);
                            _consumer.Commit(result);
                            break;
                        }
                        
                        var partitions = _kafkaAdmin.GetPartitions();
        
                        // Находим текущую партицию
                        var currentPartition = result.Partition.Value; // Получаем текущую партицию из сообщения

                        // Ищем другую партицию для перенаправления
                        var otherPartitions = partitions.Where(p => p.PartitionId != currentPartition).ToList();
                        

                        if (otherPartitions.Any())
                        {
                            // Выбираем случайную другую партицию
                            var targetPartition = otherPartitions[new Random().Next(otherPartitions.Count)];

                            responce.Score++;
                            jsonmessage = JsonConvert.SerializeObject(responce);

                            await _rederectMessageProducer.Message(type, jsonmessage, targetPartition.PartitionId);

                            // Отправляем сообщение в другую партицию
                            break;
                        }
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
