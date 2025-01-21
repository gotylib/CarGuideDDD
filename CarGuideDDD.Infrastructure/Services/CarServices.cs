using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using Microsoft.AspNetCore.Identity;
using CarGuideDDD.Core.DomainObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.MailSendObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;
using CarGuideDDD.Core.AnswerObjects;

namespace CarGuideDDD.Infrastructure.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<EntityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ProducerHostedService _producerHostedService;

        public CarService(
            ICarRepository carRepository,
            RoleManager<IdentityRole> roleManager,
            UserManager<EntityUser> userManager,
            IUserRepository userRepository,
            ProducerHostedService producerHostedService)
        {
            _carRepository = carRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _producerHostedService = producerHostedService;
        }

        // Получение всех автомобилей
        public IQueryable<PriorityCarDto> GetAllCars() { return _carRepository.GetAll(true); }

        public IQueryable<PriorityCarDto> GetForAllCars() { return _carRepository.GetAll(false); }

        // Получение автомобиля по ID
        public async Task<PriorityCarDto> GetCarByIdAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if(car == null)
            {
                throw new KeyNotFoundException($"Car with ID {id} not found.");
            }
            return car;
        }

        // Добавление нового автомобиля
        public async Task AddCarAsync(PriorityCarDto car)
        {
            if(car == null)
            {
                throw new ArgumentNullException(nameof(car), "Car cannot be null.");
            }

            await _carRepository.AddAsync(car);
        }

        public async Task AddPhotoToCarAsync(CarPhotoDto carPhoto, string guid)
        {
            if(carPhoto == null)
            {
                throw new ArgumentNullException(nameof(carPhoto), "Car cannot be null.");
            }

            await _carRepository.AddCarPhotoAsync(carPhoto, guid);

        }  

        // Обновление существующего автомобиля
        public async Task UpdateCarAsync(int id, PriorityCarDto car)
        {
            if(car == null)
            {
                throw new ArgumentNullException(nameof(car), "Car cannot be null.");
            }

            var existingCar = await _carRepository.GetByIdAsync(id);
            if(existingCar == null)
            {
                throw new KeyNotFoundException($"Car with ID {car.Id} not found.");
            }

            await _carRepository.UpdateAsync(id, car);
        }

        // Удаление автомобиля
        public async Task DeleteCarAsync(int id)
        {
            var existingCar = await _carRepository.GetByIdAsync(id);
            if(existingCar == null)
            {
                throw new KeyNotFoundException($"Car with ID {id} not found.");
            }

            await _carRepository.DeleteAsync(id);
        }

        // Изминение колличества автомобилей

        public async Task UpdateCarQuantityAsync(int id, int quantity)
        { await _carRepository.UpdateQuantityAsync(id, quantity); }

        // Сделать автомобиль недоступным
        public async Task<ServiceResult> SetCarAvailabilityAsync(int id, bool inAvailable)
        {
            var car = await _carRepository.GetByIdAsync(id);

            var resultAction = Maps.MapPriorityCarDtoToCar(car).SetCarAvailability();

            switch(resultAction)
            {
                case AvailabilityActionResult.Success:
                    await _carRepository.SetAvailabilityAsync(id, inAvailable);
                    return ServiceResult.Ok();
                case AvailabilityActionResult.InvalidStockCount:
                    return ServiceResult.BadRequest("Машину можно сделать недоступной, только если их нет на складе");
                default:
                    return ServiceResult.ServerError();
            }
        }

        public async Task<bool> BuyAsync(int id, string clientName)
        {
            var client = await _userRepository.GetByNameAsync(clientName);
            var car = await _carRepository.GetByIdAsync(id);
            var managers = (await _userManager.GetUsersInRoleAsync("Manager")).Select(Maps.MapEntityUserToUser).ToList();
            var result = Maps.MapPriorityCarDtoToCar(car).BuyCar(managers, Maps.MapUserDtoToUser(client));
            switch (result.Status)
            {
                case BuyCarActionResult.SendBuyMessage:
                {
                    var resultAnswer = result is { Manager: not null, Client: not null };
                    var sendMessage = new MailSendObj
                    {
                        EventId = (int)BuyCarActionResult.SendBuyMessage + 1,
                        User = Maps.MapUserToMailUser(result.Client),
                        Manager = Maps.MapUserToMailUser(result.Manager),
                        Car = Maps.MapPriorityCarDotToMailCar(car),
                        Score = 1
                    };
                    var send = JsonConvert.SerializeObject(sendMessage);
                    _producerHostedService.SendMessage(send, "MailMessages");
                    return resultAnswer;
                }
                case BuyCarActionResult.SendErrorMessageNoHaveManagers:
                {
                    var resultAnswer = result.Client != null;
                    var sendMessaga = new MailSendObj
                    {
                        EventId = (int)BuyCarActionResult.SendErrorMessageNoHaveManagers,
                        User = Maps.MapUserToMailUser(result.Client),
                        Score = 1
                    };
                    var send = JsonConvert.SerializeObject(sendMessaga);
                    _producerHostedService.SendMessage(send, "MailMessages");
                    return resultAnswer;
                }
                case BuyCarActionResult.SendErrorMessageNoHaveCar:
                {
                    var resultAnswer = result.Client != null;
                    var sendMessaga = new MailSendObj
                    {
                        EventId = (int)BuyCarActionResult.SendErrorMessageNoHaveCar,
                        User = Maps.MapUserToMailUser(result.Client),
                        Car = Maps.MapPriorityCarDotToMailCar(car),
                        Score = 1
                    };
                    var send = JsonConvert.SerializeObject(sendMessaga);
                    _producerHostedService.SendMessage(send, "MailMessages");
                    return resultAnswer;
                }
                default:
                    return false;
            }
        }

        public async Task<bool> InfoAsync(int id, string clientName)
        {
            var client = await _userRepository.GetByNameAsync(clientName);
            var car = await _carRepository.GetByIdAsync(id);
            var managers = (await _userManager.GetUsersInRoleAsync("Manager")).Select(Maps.MapEntityUserToUser).ToList();
            var result = Maps.MapPriorityCarDtoToCar(car).InfoCar(managers, Maps.MapUserDtoToUser(client));

            switch (result.Status)
            {
                case InfoCarActionResult.SendInfoMessage:
                {
                    var resultAnswer = result is { Manager: not null, Client: not null };
                    var sendMessage = new MailSendObj
                    {
                        EventId = (int)InfoCarActionResult.SendInfoMessage,
                        User = Maps.MapUserToMailUser(result.Client),
                        Manager = Maps.MapUserToMailUser(result.Manager),
                        Car = Maps.MapPriorityCarDotToMailCar(car),
                        Score = 1
                    };
                    var send = JsonConvert.SerializeObject(sendMessage);
                    _producerHostedService.SendMessage(send, "MailMessages");
                    return resultAnswer;
                }
                case InfoCarActionResult.SendErrorMessageNoHaveManagers:
                {
                    var sendMessaga = new MailSendObj
                    {
                        EventId = (int)InfoCarActionResult.SendErrorMessageNoHaveManagers,
                        User = Maps.MapUserToMailUser(result.Client),
                        Score = 1
                    };
                    var send = JsonConvert.SerializeObject(sendMessaga);
                    _producerHostedService.SendMessage(send, "MailMessages");
                    return true;
                }
                case InfoCarActionResult.SendErrorMessageNoHaveCar:
                {
                    var sendMessaga = new MailSendObj
                    {
                        EventId = (int)InfoCarActionResult.SendErrorMessageNoHaveCar,
                        User = Maps.MapUserToMailUser(result.Client),
                        Car = Maps.MapPriorityCarDotToMailCar(car),
                        Score = 1
                    };
                    var send = JsonConvert.SerializeObject(sendMessaga);
                    _producerHostedService.SendMessage(send, "MailMessages");
                    return true;
                }
                default:
                    return false;
            }
        }
    }
}


