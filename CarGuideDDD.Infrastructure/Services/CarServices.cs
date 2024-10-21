using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Domain.Methods;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using Domain.Entities;
using DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

namespace CarGuideDDD.Infrastructure.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<EntityUser> _userManager;
        private readonly IUserRepository _userRepository;

        public CarService(ICarRepository carRepository, RoleManager<IdentityRole> roleManager, UserManager<EntityUser> userManager, IUserRepository userRepository)
        {
            _carRepository = carRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _userRepository = userRepository;
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
        public async Task SetCarAvailabilityAsync(int id, bool inAvailable)
        {
            await _carRepository.SetAvailabilityAsync(id, inAvailable);
        }

        public async Task<bool> BuyOrInforameAsync(int id, string clientName,bool statis)
        {
            var users = _userManager.Users.ToList();

            // Список для хранения пользователей с ролью "Manager"
            var managers = new List<EntityUser>();

            foreach (var user in users)
            {
                // Проверяем, есть ли у пользователя роль "Manager"
                if (await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    managers.Add(user);
                }
            }
            Random random = new Random();
            
            CreateRequestCar createRequestCar = new CreateRequestCar();
            var client = await _userRepository.GetByNameAsync(clientName);    
            var car = await _carRepository.GetByIdAsync(id);
            if(managers.Count == 0)
            {
                return false;
            }
            else
            {
                return await createRequestCar.CreatePurchaseRequestOrGetInformationAboutCar(car, client, Maps.MapEntityUseToUserDto(managers[random.Next(managers.Count)]), statis);
            }

        }

    }
}
