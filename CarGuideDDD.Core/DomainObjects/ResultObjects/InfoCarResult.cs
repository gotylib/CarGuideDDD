namespace CarGuideDDD.Core.DomainObjects.ResultObjects
{
    public class InfoCarResult
    {
        public InfoCarResult(User client, InfoCarActionResult status)
        {
            Client = client;
            Status = status;
        }
        
        public InfoCarActionResult Status { get; }
        public User? Manager { get; init; }

        public User Client { get; }
    }
}
