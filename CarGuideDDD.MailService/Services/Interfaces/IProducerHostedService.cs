namespace CarGuideDDD.MailService.Services.Interfaces
{
    public interface IProducerHostedService
    {
        void SendMessage(object obj, string queueName);

        void SendMessage(string message, string queueName);
    }
}
