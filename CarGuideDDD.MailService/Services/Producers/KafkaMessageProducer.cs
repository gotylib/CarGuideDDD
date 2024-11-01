using Confluent.Kafka;

namespace CarGuideDDD.MailService.Services.Producers;

public sealed class KafkaMessageProducer(
    IProducer<int, string> producer,
    string topic,
    ILogger<KafkaMessageProducer> logger,
    int partition)
{
    private readonly IProducer<int, string> _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
    private readonly ILogger<KafkaMessageProducer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly int _partition = partition;

    public async Task Message(int key, string message)
    {
        _logger.LogInformation("Kafka message sending...");
        await _producer.ProduceAsync
        (
            topic: _topic,
            message: new Message<int, string>
            {
                Key = key,
                Value = message,
            }
        );
    }


    private void HandleDeliveryReport(DeliveryReport<int, string> report)
    {
        switch (report.Status)
        {
            case PersistenceStatus.NotPersisted:
                _logger.LogWarning("Sending kafka message failed.");
                break;
            case PersistenceStatus.PossiblyPersisted:
                _logger.LogWarning("Sending kafka message possibly failed.");
                break;
            case PersistenceStatus.Persisted:
                _logger.LogInformation("Kafka message sent.");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}