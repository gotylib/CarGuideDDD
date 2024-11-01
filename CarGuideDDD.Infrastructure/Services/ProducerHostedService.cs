using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CarGuideDDD.Infrastructure.Services.Producers;

namespace CarGuideDDD.Infrastructure.Services
{
    public sealed class ProducerHostedService : IDisposable
    {
        private readonly KafkaMessageProducer _producer;
        private readonly ILogger<ProducerHostedService> _logger;


        public ProducerHostedService(KafkaMessageProducer producer, ILogger<ProducerHostedService> logger)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void Dispose(){}

        public void SendMessage(int key,string message)
        {
            _logger.LogInformation("Producer sending message to kafka...");
            _producer.Message(key, message);
            _logger.LogInformation("Producer sent message to kafka.");
        }
    }
}
