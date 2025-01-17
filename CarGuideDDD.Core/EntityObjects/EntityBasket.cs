

using System.ComponentModel.DataAnnotations;

namespace CarGuideDDD.Core.EntityObjects
{
    public class EntityBasket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public List<EntityCar>? Cars { get; set; }

    }
}
