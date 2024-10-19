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

        public CarService(ICarRepository carRepository, RoleManager<IdentityRole> roleManager)
        {
            _carRepository = carRepository;
            _roleManager = roleManager;
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

    }
}
