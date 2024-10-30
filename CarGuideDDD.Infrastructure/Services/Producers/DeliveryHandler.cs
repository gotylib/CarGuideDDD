using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace CarGuideDDD.Infrastructure.Services.Producers;

public static class DeliveryHandlers<T>
{
    public static void DeliveryHandler(DeliveryReport<Null, string> report, ILogger<T> logger)
    {
        switch (report.Status)
        {
            case PersistenceStatus.NotPersisted:
                logger.LogWarning("Sending kafka message failed.");
                break;
            case PersistenceStatus.PossiblyPersisted:
                logger.LogWarning("Sending kafka message possibly failed.");
                break;
            case PersistenceStatus.Persisted:
                logger.LogInformation("Kafka message sent.");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}