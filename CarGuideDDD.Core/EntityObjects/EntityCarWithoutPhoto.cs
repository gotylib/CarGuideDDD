
using CarGuideDDD.Core.EntityObjects.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityCarWithoutPhoto : IEntity
    {
        [Key]
        public int Id { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public DateTime AddTime { get; set; }
        public string? AddUserName { get; set; }
    }
}
