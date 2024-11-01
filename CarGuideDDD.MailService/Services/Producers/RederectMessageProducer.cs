using Confluent.Kafka;

namespace CarGuideDDD.MailService.Services.Producers;

public class RederectMessageProducer(
    IProducer<int, string> producer,
    string topic,
    ILogger<ProducerConfig> logger)
{
    private readonly IProducer<int, string> _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
    private readonly ILogger<ProducerConfig> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Message(int key, string message, int? partition)
    {
        if (partition == null)
        {
            _logger.LogInformation("Kafka message rederectiong...");
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
        else
        {
            var topicPartition = new TopicPartition(_topic, new Partition(partition.Value));
            var kafkaMessage = new Message<int, string>
            {
                Key = key,
                Value = message
            };

            _logger.LogInformation("Kafka message rederectiong...");
            await _producer.ProduceAsync(topicPartition, kafkaMessage);
        }
    }
}

