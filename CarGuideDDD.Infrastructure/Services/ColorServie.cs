using CarGuideDDD.Core.AnswerObjects;
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

        public async Task<ServiceResult> AddColorAsync(ColorDto colorDto)
        {
            if (colorDto == null || colorDto.Color.IsNullOrEmpty())
            {
                return ServiceResult.BadRequest("Объект задан неправильно");
            }

            return await _colorRepository.AddAsync(colorDto) 
                ? ServiceResult.Ok("Цвет был добавлен")
                : ServiceResult.ServerError();
        }

        public async Task<ServiceResult> DeleteColorAsync(IdDto id)
        {
            if (id.Id == 0)
            {
                return ServiceResult.BadRequest("Id задан не правильно"); ;
            }

            return await _colorRepository.DeleteAsync(id)
                ? ServiceResult.Ok("Цвет был удалён")
                : ServiceResult.ServerError();
        }

        public async Task<ServiceResultGet<ColorDto,Exception,VoidDto>> GetColorAsync()
        {

            var result = await _colorRepository.GetAllAsync();
            
            return result.IsSuccessful
                ? ServiceResultGet<ColorDto, Exception, VoidDto>.IEnumerableResult(result.Value)
                : ServiceResultGet<ColorDto, Exception, VoidDto>.ErrorResult(result.Error);
        }

        public async Task<ServiceResult> UpdateColorAsync(ColorDto colorDto)
        {
            if (colorDto == null || colorDto.Color.IsNullOrEmpty())
            {

                return ServiceResult.BadRequest("Объект задан неправильно");
            }

            return await _colorRepository.UpdateAsync(colorDto)
                ? ServiceResult.Ok("Цвет был обновлён")
                : ServiceResult.ServerError();

        }
    }
}
