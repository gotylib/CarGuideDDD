using Confluent.Kafka;
using Microsoft.Extensions.Logging;


namespace CarGuideDDD.Infrastructure.Services.Producers
{
    public sealed class KafkaMessageProducer(
        IProducer<int, string> producer,
        string topic,
        ILogger<KafkaMessageProducer> logger)
    {
        private readonly IProducer<int, string> _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        private readonly ILogger<KafkaMessageProducer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void Message(int key,string message)
        {
            _logger.LogInformation("Kafka message sending...");
            _producer.Produce
            (
                topic: _topic,
                message: new Message<int, string>
                {
                    Key = key,
                    Value = message,
                },
                deliveryHandler: HandleDeliveryReport
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
}
