using Domain.Entities;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
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
        public async Task<IEnumerable<Car>> GetAllCarsAsync()
        {
            var listUser  = (await  _carRepository.GetAllAsync(true)).ToList();
            List<Car> cars = new List<Car>();
            listUser.ForEach(car => cars.Add(EntityCar.ConvertToCar(car)));
            return cars;
        }

        public async Task<IEnumerable<Car>> GetForAllCarsAsync()
        {
            var listUser = (await _carRepository.GetAllAsync(false)).ToList();
            List<Car> cars = new List<Car>();
            listUser.ForEach(car => cars.Add(EntityCar.ConvertToCar(car)));
            return cars;
        }

        // Получение автомобиля по ID
        public async Task<Car> GetCarByIdAsync(int id)
        {
            var car = EntityCar.ConvertToCar( await _carRepository.GetByIdAsync(id));
            if (car == null)
            {
                throw new KeyNotFoundException($"Car with ID {id} not found.");
            }
            return car;
        }

        // Добавление нового автомобиля
        public async Task AddCarAsync(Car car)
        {
            if (car == null)
            {
                throw new ArgumentNullException(nameof(car), "Car cannot be null.");
            }

            await _carRepository.AddAsync(car);
        }

        // Обновление существующего автомобиля
        public async Task UpdateCarAsync(int id, Car car)
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

            await _carRepository.UpdateAsync(id,car);
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
