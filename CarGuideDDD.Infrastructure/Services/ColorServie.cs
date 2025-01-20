using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CarGuideDDD.Infrastructure.Services
{
    public class ColorServie : IColorService
    {
        IColorRepository _colorRepository;
        ILogger<ColorServie> _logger;

        public ColorServie(IColorRepository colorRepository, ILogger<ColorServie> logger)
        {
            _colorRepository = colorRepository;
            _logger = logger;
        }

        public async Task<IActionResult> AddColorAsync(ColorDto colorDto)
        {
            if(colorDto == null || colorDto.Color.IsNullOrEmpty())
            {
                return new BadRequestObjectResult("Объект задан неправильно");
            }

            return await _colorRepository.AddAsync(colorDto) 
                ? new OkObjectResult("Цвет был добавлен")
                : new StatusCodeResult(500);
        }

        public async Task<IActionResult> DeleteColorAsync(IdDto id)
        {
            if (id.Id == 0)
            {
                return new BadRequestObjectResult("Id задан не правильно"); ;
            }

            return await _colorRepository.DeleteAsync(id)
                ? new OkObjectResult("Цвет был удалён")
                : new StatusCodeResult(500);
        }

        public async Task<IActionResult> GetColorAsync()
        {

            var result = await _colorRepository.GetAllAsync();
            
            return result.IsSuccessful
                ? new OkObjectResult(result.Value)
                : new BadRequestObjectResult(result.Error);
        }

        public async Task<IActionResult> UpdateColorAsync(ColorDto colorDto)
        {
            if (colorDto == null || colorDto.Color.IsNullOrEmpty())
            {

                return new BadRequestObjectResult("Объект задан неправильно");
            }

            return await _colorRepository.UpdateAsync(colorDto)
                ? new OkObjectResult("Цвет был обновлён")
                : new StatusCodeResult(500);

        }
    }
}
