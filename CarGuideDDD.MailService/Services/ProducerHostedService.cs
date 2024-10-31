using CarGuideDDD.MailService.Services.Producers;

namespace CarGuideDDD.MailService.Services;

public sealed class ProducerHostedService(KafkaMessageProducer producer, ILogger<ProducerHostedService> logger)
    : IHostedService, IDisposable
{
    private readonly KafkaMessageProducer _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    private readonly ILogger<ProducerHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Producer starting...");

        _logger.LogInformation("Producer started.");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Producer stopping...");

        _logger.LogInformation("Producer stopped.");

        return Task.CompletedTask;
    }

    public void Dispose(){}

    public void SendMessage(int key,string message)
    {
        _logger.LogInformation("Producer sending message to kafka...");
        _producer.Message(key, message);
        _logger.LogInformation("Producer sent message to kafka.");
    }
}