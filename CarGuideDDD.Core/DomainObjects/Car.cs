﻿using CarGuideDDD.Core.DomainObjects.ResultObjects;
using DTOs;


namespace Domain.Entities
{

    public enum AvailabilityActionResult
    {
        Success,
        InvalidStockCount
    }

    public enum BuyCarActionResult
    {
        SendErrorMessageToUser,
        SendBuyMessage,
    }

    public enum InfoCarActionResult 
    {
        SendErrorMessageToUser,
        SendInfoMessage,
    }


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

        public AvailabilityActionResult SetCarAvailability()
        {
            if (StockCount != 0)
            {
                return AvailabilityActionResult.InvalidStockCount;
            }
            else
            {
                return AvailabilityActionResult.Success;
            }
        }

        public BuyCarResult BuyCar(IList<User> managers, User client)
        {
            if(managers.Count == 0 || StockCount == 0)
            {
                return new BuyCarResult {Status = BuyCarActionResult.SendErrorMessageToUser, Manager = null , Client = client};
            }
            else
            {
                Random random = new Random();
                var manager = managers[random.Next(managers.Count)];
                return new BuyCarResult { Status = BuyCarActionResult.SendBuyMessage, Manager = manager, Client = client };
            }
        }

        public InfoCarResult InfoCar(IList<User> managers, User client)
        {
            if (managers.Count == 0 || StockCount == 0)
            {
                return new InfoCarResult { Status = InfoCarActionResult.SendErrorMessageToUser, Manager = null, Client = client };
            }
            else
            {
                Random random = new Random();
                var manager = managers[random.Next(managers.Count)];
                return new InfoCarResult { Status = InfoCarActionResult.SendInfoMessage, Manager = manager, Client = client };
            }
        }
    }
}
