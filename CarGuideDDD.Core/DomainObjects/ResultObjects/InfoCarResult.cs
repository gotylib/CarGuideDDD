

using Domain.Entities;

namespace CarGuideDDD.Core.DomainObjects.ResultObjects
{
    public class InfoCarResult
    {
        public InfoCarActionResult Status { get; set; }
        public User? Manager { get; set; }

        public User Client { get; set; }
    }
}
