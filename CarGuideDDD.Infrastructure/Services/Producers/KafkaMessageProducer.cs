using Confluent.Kafka;
using Microsoft.Extensions.Logging;


namespace CarGuideDDD.Infrastructure.Services.Producers
{
    public sealed class KafkaMessageProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaMessageProducer> _logger;

        public KafkaMessageProducer(IProducer<Null, string> producer, string topic,
            ILogger<KafkaMessageProducer> logger)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public void Message(string message)
        {
            _logger.LogInformation("Kafka message sending...");
            _producer.Produce
            (
              topic: _topic,
              message: new Message<Null, string>
              {
                  Value = message,
              },
              deliveryHandler: (DeliveryReport<Null, string> report) =>
              {
                  
                  switch (report.Status)
                  {
                      case PersistenceStatus.NotPersisted:
                          _logger.LogWarning("Sending kafka message failed.");
                          return;
                      case PersistenceStatus.PossiblyPersisted:
                          _logger.LogWarning("Sending kafka message possibly failed.");
                          return;
                      case PersistenceStatus.Persisted:
                          _logger.LogInformation("Kafka message sent.");
                          return;
                      default:
                          throw new ArgumentOutOfRangeException();
                  }
              }
            );
        }
    }
}
