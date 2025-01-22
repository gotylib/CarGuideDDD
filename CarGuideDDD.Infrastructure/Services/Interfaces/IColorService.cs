
using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;


namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IColorService
    {
        // Добавление цвета
        Task<ServiceResult<VoidDto, Exception, VoidDto>> AddColorAsync(ColorDto color);

        // Редактирование цвета
        Task<ServiceResult<VoidDto, Exception, VoidDto>> UpdateColorAsync(ColorDto colorDto);

        // Удаление цвета
        Task<ServiceResult<VoidDto, Exception, VoidDto>> DeleteColorAsync(IdDto id);

        // Получение всех цветов
        Task<ServiceResult<ColorDto, Exception, VoidDto>> GetColorAsync();
    }
}
