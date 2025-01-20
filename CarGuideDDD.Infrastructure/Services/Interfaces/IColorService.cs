

using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IColorService
    {
        //Добавление цвета
        Task<IActionResult> AddColorAsync(ColorDto color);
        //Редактирование цвета
        Task<IActionResult> UpdateColorAsync(ColorDto colorDto);
        //Удаление цвета
        Task<IActionResult> DeleteColorAsync(IdDto id);

        Task<IActionResult> GetColorAsync();
    }
}
