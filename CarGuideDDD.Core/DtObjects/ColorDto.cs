using CarGuideDDD.Core.EntityObjects.Interfaces;

namespace CarGuideDDD.Core.DtObjects
{
    public class ColorDto : IEntity
    {
        public int Id { get; set; }
        public string? Color { get; set; }
    }
}
