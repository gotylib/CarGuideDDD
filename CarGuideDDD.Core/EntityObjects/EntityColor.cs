using CarGuideDDD.Core.EntityObjects.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityColor : IEntity
    {
        [Key]
        public int Id {  get; set; }    

        public string? Color { get; set; }
    }
}
