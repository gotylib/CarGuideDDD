using System.ComponentModel.DataAnnotations;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityCar
    {
        [Key]
        public int Id { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public EntityColor? Color { get; set; }
        public DateTime AddTime { get; set; }
        public string? AddUserName { get; set; }
        public string? NameOfPhoto { get; set; }
        public int StockCount { get; set; }
        public bool IsAvailable { get; set; }
    }
}
