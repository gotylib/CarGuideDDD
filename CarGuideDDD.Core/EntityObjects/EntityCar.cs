using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data
{
    public class EntityCar
    {
        [Key]
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public int StockCount { get; set; }
        public bool IsAvailable { get; set; }
    }
}
