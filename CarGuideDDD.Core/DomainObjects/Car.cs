using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DomainObjects.ResultObjects;
using CarGuideDDD.Core.DtObjects;

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

    public enum RoleBasketGet
    {
        None,
        User,
        Manager,
        Admin,
    }

    public enum RoleBasketCDU
    {
        Manager,
        NonAdmin,
        Default
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

        public bool Create(DateTime addTime, string make, string model, string color, int stockCount, bool isAvailable, string addUserName = "", string nameOfPhoto = "", List<string> roles = null)
        {
            if (string.IsNullOrEmpty(addUserName) || !IsInRole(roles))
            {
                return false;
            }

            Make = make;
            Model = model;
            Color = color;
            StockCount = stockCount;
            IsAvailable = isAvailable;
            AddUserName = addUserName;
            AddTime = addTime;
            NameOfPhoto = nameOfPhoto;

            return true; ;
        }

        public bool Update(DateTime addTime, string make, string model, string color, int stockCount, bool isAvailable, string addUserName = "", string nameOfPhoto = "", List<string> roles = null)
        {
            if (string.IsNullOrEmpty(addUserName) || !IsInRole(roles))
            {
                return false;
            }

            Make = make;
            Model = model;
            Color = color;
            StockCount = stockCount;
            IsAvailable = isAvailable;
            AddUserName = addUserName;
            AddTime = addTime;
            NameOfPhoto = nameOfPhoto;

            return true;
        }

        public bool Delete(List<string> roles = null)
        {
            if (!IsInRole(roles))
            {
                return false;
            }

            // Логика удаления
            return true;
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

        private bool IsInRole(List<string> roles)
        {
            return (roles.Contains("Manager") || roles.Contains("Admin"))
                ? true 
                : false;
            
        }
    }
}
