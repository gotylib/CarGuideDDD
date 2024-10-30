using CarGuideDDD.Infrastructure.Services.Producers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace CarGuideDDD.Infrastructure.Services
{
    public sealed class ProducerHostedService : IHostedService, IDisposable
    {
        private readonly KafkaMessageProducer _producer;
        private readonly ILogger<ProducerHostedService> _logger;


        public ProducerHostedService(KafkaMessageProducer producer, ILogger<ProducerHostedService> logger)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
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

        public void SendMessage(string message)
        {
            _logger.LogInformation("Producer sending message to kafka...");
            _producer.Message(message);
            _logger.LogInformation("Producer sent message to kafka.");
        }
    }
}
