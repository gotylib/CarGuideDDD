using Newtonsoft.Json;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using Microsoft.AspNetCore.Identity;
using CarGuideDDD.Core.DomainObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.MailSendObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Infrastructure.Services.Interfaces;

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
        public async Task<ServiceResult<VoidDto, Exception, PriorityCarDto>> GetCarByIdAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return ServiceResult<VoidDto, Exception, PriorityCarDto>.ErrorResult(new KeyNotFoundException($"Car with ID {id} not found."));
            }
            return ServiceResult<VoidDto, Exception, PriorityCarDto>.SimpleResult(car);
        }

        // Добавление нового автомобиля
        public async Task<ServiceResult<VoidDto, Exception, PriorityCarDto>> AddCarAsync(PriorityCarDto car, List<string> roles)
        {
            if (car == null)
            {
                return ServiceResult<VoidDto, Exception, PriorityCarDto>.BadRequest();
            }

            var carEntity = new Car();
            var createResult = carEntity.Create(DateTime.UtcNow, car.Make, car.Model, car.Color, car.StockCount, car.IsAvailable, car.AddUserName, car.NameOfPhoto, roles);
            if (!createResult)
            {
                return ServiceResult<VoidDto, Exception, PriorityCarDto>.BadRequest();
            }

            await _carRepository.AddAsync(car);
            return ServiceResult<VoidDto, Exception, PriorityCarDto>.SimpleResult(car);
        }

        public async Task<ServiceResult<VoidDto, Exception, CarPhotoDto>> AddPhotoToCarAsync(CarPhotoDto carPhoto, string guid)
        {
            if (carPhoto == null)
            {
                return ServiceResult<VoidDto, Exception, CarPhotoDto>.BadRequest();
            }

            await _carRepository.AddCarPhotoAsync(carPhoto, guid);
            return ServiceResult<VoidDto, Exception, CarPhotoDto>.SimpleResult(carPhoto);
        }

        // Обновление существующего автомобиля
        public async Task<ServiceResult<VoidDto, Exception, PriorityCarDto>> UpdateCarAsync(int id, PriorityCarDto car, List<string> roles)
        {
            if (car == null)
            {
                return ServiceResult<VoidDto, Exception, PriorityCarDto>.BadRequest();
            }

            var existingCar = await _carRepository.GetByIdAsync(id);
            if (existingCar == null)
            {
                return ServiceResult<VoidDto, Exception, PriorityCarDto>.BadRequest();
            }

            var domainCar = new Car();
            var updateResult = domainCar.Update(DateTime.UtcNow, car.Make, car.Model, car.Color, car.StockCount, car.IsAvailable, car.AddUserName, car.NameOfPhoto, roles);
            if (!updateResult)
            {
                return ServiceResult<VoidDto, Exception, PriorityCarDto>.BadRequest();
            }

            await _carRepository.UpdateAsync(id, car);
            return ServiceResult<VoidDto, Exception, PriorityCarDto>.SimpleResult(car);
        }

        // Удаление автомобиля
        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> DeleteCarAsync(int id, List<string> roles)
        {
            var existingCar = await _carRepository.GetByIdAsync(id);
            if (existingCar == null)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.ErrorResult(new KeyNotFoundException($"Car with ID {id} not found."));
            }
            var domainCar = new Car();
            var deleteResult = domainCar.Delete(roles);
            if (!deleteResult)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            await _carRepository.DeleteAsync(id);
            return ServiceResult<VoidDto, Exception, VoidDto>.Ok();
        }

        // Изминение колличества автомобилей
        public async Task UpdateCarQuantityAsync(int id, int quantity)
        {
            await _carRepository.UpdateQuantityAsync(id, quantity);
        }

        // Сделать автомобиль недоступным
        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> SetCarAvailabilityAsync(int id, bool inAvailable)
        {
            var car = await _carRepository.GetByIdAsync(id);

            var resultAction = Maps.MapPriorityCarDtoToCar(car).SetCarAvailability();

            switch (resultAction)
            {
                case AvailabilityActionResult.Success:
                    await _carRepository.SetAvailabilityAsync(id, inAvailable);
                    return ServiceResult<VoidDto, Exception, VoidDto>.Ok();
                case AvailabilityActionResult.InvalidStockCount:
                    return ServiceResult<VoidDto, Exception, VoidDto>.ErrorResult(new InvalidOperationException("Машину можно сделать недоступной, только если их нет на складе"));
                default:
                    return ServiceResult<VoidDto, Exception, VoidDto>.ServerError();
            }
        }

        public async Task<ServiceResult<VoidDto, Exception, BooleanResultDto>> BuyAsync(int id, string clientName)
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
                        return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = resultAnswer });
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
                        return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = resultAnswer });
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
                        return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = resultAnswer });
                    }
                default:
                    return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = false });
            }
        }

        public async Task<ServiceResult<VoidDto, Exception, BooleanResultDto>> InfoAsync(int id, string clientName)
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
                        return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = resultAnswer });
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
                        return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = true });
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
                        return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = true });
                    }
                default:
                    return ServiceResult<VoidDto, Exception, BooleanResultDto>.SimpleResult(new BooleanResultDto { Value = false });
            }
        }
    }

}


