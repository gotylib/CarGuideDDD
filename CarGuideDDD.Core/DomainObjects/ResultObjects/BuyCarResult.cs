using Domain.Entities;

namespace CarGuideDDD.Core.DomainObjects.ResultObjects
{
    public class BuyCarResult
    {
        public BuyCarActionResult Status { get; set; }
        public User? Manager { get; set; }

        public User Client { get; set; }
    }
}
