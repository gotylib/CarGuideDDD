using DTOs;
using Microsoft.AspNetCore.Mvc;


namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface ICarServices
    {
        public interface ICarService
        {
            // Получение всех автомобилей
            IQueryable<PriorityCarDto> GetAllCars();

            IQueryable<PriorityCarDto> GetForAllCars();

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

            //Создать заявку на покупку машины
            Task<bool> BuyAsync(int id, string clientName);

            //Создать заявку на получение информации о машине
            Task<bool> InfoAsync(int id, string clientName);
        }
    }
}
