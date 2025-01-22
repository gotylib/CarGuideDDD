using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.AnswerObjects;

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
            Task<ServiceResult> AddCarAsync(PriorityCarDto car, List<string> roles);

            // Обновление существующего автомобиля
            Task<ServiceResult> UpdateCarAsync(int id, PriorityCarDto car, List<string> roles);

            // Удаление автомобиля
            Task<ServiceResult> DeleteCarAsync(int id, List<string> roles);

            // Изменение количества автомобилей
            Task UpdateCarQuantityAsync(int id, int quantity);

            // Сделать автомобиль недоступным
            Task<ServiceResult> SetCarAvailabilityAsync(int id, bool inAvailable);

            //Создать заявку на покупку машины
            Task<bool> BuyAsync(int id, string clientName);

            //Создать заявку на получение информации о машине
            Task<bool> InfoAsync(int id, string clientName);

            //Добавление фото для машины
            public Task<ServiceResult> AddPhotoToCarAsync(CarPhotoDto carPhoto, string guid);
        }
    }
}
