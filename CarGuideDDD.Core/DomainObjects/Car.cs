using DTOs;


namespace Domain.Entities
{
    
    public class Car 
    { 
        public string Make { get; private set; }
        public string Model { get; private set; }
        public string Color { get; private set; }
        public int StockCount { get; private set; }
        public bool IsAvailable { get; private set; }

        public void Create(string make, string model, string color, int stockCount, bool isAvailable)
        {
            Make = make;
            Model = model;
            Color = color;
            StockCount = stockCount;
            IsAvailable = isAvailable;
        }
    }
}
