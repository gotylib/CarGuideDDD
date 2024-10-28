using Confluent.Kafka;
using Microsoft.Extensions.Logging;


namespace CarGuideDDD.Infrastructure.Services.Producers
{
    public sealed class KafkaMessageProducer(IProducer<Null, string> producer, string topic, ILogger<KafkaMessageProducer> logger)
    {
        private readonly IProducer<Null, string> _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        private readonly ILogger<KafkaMessageProducer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                  if (report is null)
                  {
                      _logger.LogWarning("Kafka delivery report null.");
                      return;
                  }

                  if (report.Status == PersistenceStatus.NotPersisted)
                  {
                      _logger.LogWarning("Sending kafka message failed.");
                      return;
                  }

                  if (report.Status == PersistenceStatus.PossiblyPersisted)
                  {
                      _logger.LogWarning("Sending kafka message possibly failed.");
                      return;
                  }

                  if (report.Status == PersistenceStatus.Persisted)
                  {
                      _logger.LogInformation("Kafka message sent.");
                      return;
                  }
              }
            );
        }
    }
}
