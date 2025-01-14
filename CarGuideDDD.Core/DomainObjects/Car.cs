using CarGuideDDD.Core.DomainObjects.ResultObjects;

namespace CarGuideDDD.Core.DomainObjects
{

    public enum AvailabilityActionResult
    {
        Success,
        InvalidStockCount
    }

    public enum BuyCarActionResult
    {
        SendErrorMessageNoHaveCar,
        SendErrorMessageNoHaveManagers,
        SendBuyMessage,
    }

    public enum InfoCarActionResult 
    {
        SendErrorMessageNoHaveCar,
        SendErrorMessageNoHaveManagers,
        SendInfoMessage,
    }


    public class Car
    {
        public string? Make { get; private set; }
        public string? Model { get; private set; }
        public string? Color { get; private set; }
        public int StockCount { get; private set; }
        public bool IsAvailable { get; private set; }
        public string NameOfPhoto { get; private set; }
        public DateTime AddTime { get; set; }
        public string AddUserName { get; set; }
        public void Create(string make, string model, string color, int stockCount, bool isAvailable, string addUserName, DateTime AddTime, string NameOfPhoto = "")
        {
            Make = make;
            Model = model;
            Color = color;
            StockCount = stockCount;
            IsAvailable = isAvailable;
        }

        public AvailabilityActionResult SetCarAvailability()
        {
            return StockCount != 0 ? AvailabilityActionResult.InvalidStockCount : AvailabilityActionResult.Success;
        }

        public BuyCarResult BuyCar(IList<User> managers, User client)
        {
            if (managers.Count == 0)
            {
                return new BuyCarResult
                    { Status = BuyCarActionResult.SendErrorMessageNoHaveManagers, Manager = null, Client = client };
            }

            if (StockCount == 0)
            {
                return new BuyCarResult
                    { Status = BuyCarActionResult.SendErrorMessageNoHaveCar, Manager = null, Client = client };
            }

            var random = new Random();
            var manager = managers[random.Next(managers.Count)];
            return new BuyCarResult { Status = BuyCarActionResult.SendBuyMessage, Manager = manager, Client = client };
        }

        public InfoCarResult InfoCar(IList<User> managers, User client)
        {
            
            if (managers.Count == 0)
            {
                return new InfoCarResult(client, InfoCarActionResult.SendErrorMessageNoHaveManagers){Manager =  null};
            }
            if (StockCount == 0)
            {
                return new InfoCarResult(client, InfoCarActionResult.SendErrorMessageNoHaveCar) {Manager = null};
            }

            var random = new Random();
            var manager = managers[random.Next(managers.Count)];
            return new InfoCarResult(client,InfoCarActionResult.SendInfoMessage) {Manager = manager};

        }
        
    }
}
