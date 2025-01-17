using System.ComponentModel.DataAnnotations;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityColor
    {
        [Key]
        public int Id {  get; set; }    

        public string? Color { get; set; }
    }
}
