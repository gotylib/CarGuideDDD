using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.AnswerObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface ICarService
    {
        // Получение всех автомобилей
        IQueryable<PriorityCarDto> GetAllCars();

        IQueryable<PriorityCarDto> GetForAllCars();

        // Получение автомобиля по ID
        Task<ServiceResult<VoidDto, Exception, PriorityCarDto>> GetCarByIdAsync(int id);

        // Добавление нового автомобиля
        Task<ServiceResult<VoidDto, Exception, PriorityCarDto>> AddCarAsync(PriorityCarDto car, List<string> roles);

        // Обновление существующего автомобиля
        Task<ServiceResult<VoidDto, Exception, PriorityCarDto>> UpdateCarAsync(int id, PriorityCarDto car, List<string> roles);

        // Удаление автомобиля
        Task<ServiceResult<VoidDto, Exception, VoidDto>> DeleteCarAsync(int id, List<string> roles);

        // Изменение количества автомобилей
        Task UpdateCarQuantityAsync(int id, int quantity);

        // Сделать автомобиль недоступным
        Task<ServiceResult<VoidDto, Exception, VoidDto>> SetCarAvailabilityAsync(int id, bool inAvailable);

        // Создать заявку на покупку машины
        Task<ServiceResult<VoidDto, Exception, BooleanResultDto>> BuyAsync(int id, string clientName);

        // Создать заявку на получение информации о машине
        Task<ServiceResult<VoidDto, Exception, BooleanResultDto>> InfoAsync(int id, string clientName);

        // Добавление фото для машины
        Task<ServiceResult<VoidDto, Exception, CarPhotoDto>> AddPhotoToCarAsync(CarPhotoDto carPhoto, string guid);
    }
}
