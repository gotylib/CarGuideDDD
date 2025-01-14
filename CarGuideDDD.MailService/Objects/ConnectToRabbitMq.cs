using RabbitMQ.Client;

namespace CarGuideDDD.MailService.Objects
{
    public class ConnectToRabbitMq
    {
        public IConnection Connection;
        public IChannel Channel;

        public ConnectToRabbitMq(IConnection connection, IChannel channel)
        {
            Channel = channel;
            Connection = connection;   
        }
    }
}
