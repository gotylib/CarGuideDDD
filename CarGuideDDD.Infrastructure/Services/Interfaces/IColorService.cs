

using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IColorService
    {
        //Добавление цвета
        Task<ServiceResult> AddColorAsync(ColorDto color);
        //Редактирование цвета
        Task<ServiceResult> UpdateColorAsync(ColorDto colorDto);
        //Удаление цвета
        Task<ServiceResult> DeleteColorAsync(IdDto id);

        Task<ServiceResultGet<ColorDto, Exception, VoidDto>> GetColorAsync();
    }
}
