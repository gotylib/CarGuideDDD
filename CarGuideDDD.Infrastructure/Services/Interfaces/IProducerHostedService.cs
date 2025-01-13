

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IProducerHostedService
    {
        void SendMessage(object obj);

        void SendMessage(string message);
    }
}
