﻿using CarGuideDDD.Core.DomainObjects.ResultObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Domain.Entities;
using DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

namespace CarGuideDDD.Infrastructure.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<EntityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMailServices _mailServices;

        public CarService(ICarRepository carRepository, RoleManager<IdentityRole> roleManager, UserManager<EntityUser> userManager, IUserRepository userRepository, IMailServices mailServices)
        {
            _carRepository = carRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _mailServices = mailServices;
        }

        // Получение всех автомобилей
        public async Task<IEnumerable<PriorityCarDto>> GetAllCarsAsync()
        {
            return(await _carRepository.GetAllAsync(true)).ToList();
            
        }

        public async Task<IEnumerable<PriorityCarDto>> GetForAllCarsAsync()
        {
            return(await _carRepository.GetAllAsync(false)).ToList();
            
        }

        // Получение автомобиля по ID
        public async Task<PriorityCarDto> GetCarByIdAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                throw new KeyNotFoundException($"Car with ID {id} not found.");
            }
            return car;
        }

        // Добавление нового автомобиля
        public async Task AddCarAsync(PriorityCarDto car)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car), "Car cannot be null.");
            }

            await _carRepository.AddAsync(car);
        }

        // Обновление существующего автомобиля
        public async Task UpdateCarAsync(int id, PriorityCarDto car)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car), "Car cannot be null.");
            }

            var existingCar = await _carRepository.GetByIdAsync(id);
            if (existingCar == null)
            {
                throw new KeyNotFoundException($"Car with ID {car.Id} not found.");
            }

            await _carRepository.UpdateAsync(id, car);
        }

        // Удаление автомобиля
        public async Task DeleteCarAsync(int id)
        {
            var existingCar = await _carRepository.GetByIdAsync(id);
            if (existingCar == null)
            {
                throw new KeyNotFoundException($"Car with ID {id} not found.");
            }

            await _carRepository.DeleteAsync(id);
        }

        // Изминение колличества автомобилей

        public async Task UpdateCarQuantityAsync(int id, int quantity)
        {
            await _carRepository.UpdateQuantityAsync(id, quantity);
        }

        // Сделать автомобиль недоступным
        public async Task<IActionResult> SetCarAvailabilityAsync(int id, bool inAvailable)
        {
            var car = await _carRepository.GetByIdAsync(id);

            var resultAction = Maps.MapPriorityCarDtoToCar(car).SetCarAvailability();

            switch (resultAction)
            {
                case AvailabilityActionResult.Success:
                    await _carRepository.SetAvailabilityAsync(id, inAvailable);
                    return new OkResult();
                case AvailabilityActionResult.InvalidStockCount:
                    return new BadRequestObjectResult("Машину можно сделать недоступной, только если их нет на складе");
                default:
                    return new StatusCodeResult(500);

            }
        }
        public Task<bool> BuyAsync(int id, string clientName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InfoAsync(int id, string clientName)
        {
            throw new NotImplementedException();
        }

        /*public async Task<bool> BuyAsync(int id, string clientName)
        {
            var client = await _userRepository.GetByNameAsync(clientName);
            var car = await _carRepository.GetByIdAsync(id);
            var managers = (await _userManager.GetUsersInRoleAsync("Manager")).Select(Maps.MapEntityUserToUser).ToList();
            var result = Maps.MapPriorityCarDtoToCar(car).BuyCar(managers, Maps.MapUserDtoToUser(client)); 
        }

        public async Task<bool> InfoAsync(int id, string clientName)
        {
            var client = await _userRepository.GetByNameAsync(clientName);
            var car = await _carRepository.GetByIdAsync(id);
            var managers = (await _userManager.GetUsersInRoleAsync("Manager")).Select(Maps.MapEntityUserToUser).ToList();
            var result = Maps.MapPriorityCarDtoToCar(car).InfoCar(managers, Maps.MapUserDtoToUser(client));

            switch (result.Status)
            {
                case InfoCarActionResult.SendErrorMessageToUser:
                    await _mailServices.SendUserNotFountManagerMessageAsync(
            }


        }*/
    }

}


