using Confluent.Kafka;

namespace CarGuideDDD.MailService.Services.KafkaAdmin;

public class KafkaAdmin(ProducerConfig config, string topic)
{
    private readonly ProducerConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly string _topic = topic ?? throw new ArgumentNullException(nameof(topic));
    public List<PartitionMetadata> GetPartitions()
    {
        using var adminClient = new AdminClientBuilder(_config).Build();
        var metadata = adminClient.GetMetadata(_topic, TimeSpan.FromSeconds(10));
        var partitions = metadata.Topics[0].Partitions;
        return partitions;
    }
}