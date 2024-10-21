using CarGuideDDD.Core;
using Domain.Entities;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface ICarServices
    {
        public interface ICarService
        {
            // Получение всех автомобилей
            Task<IEnumerable<PriorityCarDto>> GetAllCarsAsync();

            public Task<IEnumerable<PriorityCarDto>> GetForAllCarsAsync();

            // Получение автомобиля по ID
            Task<PriorityCarDto> GetCarByIdAsync(int id);

            // Добавление нового автомобиля
            Task AddCarAsync(PriorityCarDto car);

            // Обновление существующего автомобиля
            Task UpdateCarAsync(int id, PriorityCarDto car);

            // Удаление автомобиля
            Task DeleteCarAsync(int id);

            // Изменение количества автомобилей
            Task UpdateCarQuantityAsync(int id, int quantity);

            // Сделать автомобиль недоступным
            Task<IActionResult> SetCarAvailabilityAsync(int id, bool inAvailable);

            //Создать заявку на обработку покупки или получение информации о машине
            Task<bool> BuyOrInforameAsync(int id, string clientName, bool statis);
        }
    }
}
