using Confluent.Kafka;
using System;
using System.Text.Json;

class Program
{
    private const string TopicUser = "user-notifications";
    private const string TopicManager = "manager-notifications";

    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "9092",
            GroupId = "notification_group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(new[] { TopicUser, TopicManager });

            while (true)
            {
                try
                {
                    var cr = consumer.Consume();
                    HandleMessage(cr.Value);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occurred: {e.Error.Reason}");
                }
            }
        }
    }

    private static void HandleMessage(string message)
    {
        var emailData = JsonSerializer.Deserialize<EmailData>(message);
        // Отправка email с использованием вашего метода отправки email
        Console.WriteLine($"Sending email to: {emailData.mailRecipient}");
        // Ваш код для отправки email...
    }

    private class EmailData
    {
        public string mailRecipient { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
    }
}
