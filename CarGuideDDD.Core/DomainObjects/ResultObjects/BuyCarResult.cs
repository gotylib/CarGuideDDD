namespace CarGuideDDD.Core.DomainObjects.ResultObjects
{
    public class BuyCarResult
    {
        public BuyCarActionResult Status { get; init; }
        public User? Manager { get; init; }

        public User? Client { get; init; }
    }
}
