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
        private readonly IColorRepository _colorRepository;
        private readonly ILogger<ColorServie> _logger;

        public ColorServie(IColorRepository colorRepository, ILogger<ColorServie> logger)
        {
            _colorRepository = colorRepository;
            _logger = logger;
        }

        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> AddColorAsync(ColorDto colorDto)
        {
            if (colorDto == null || string.IsNullOrEmpty(colorDto.Color))
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var success = await _colorRepository.AddAsync(colorDto);
            return success
                ? ServiceResult<VoidDto, Exception, VoidDto>.Ok()
                : ServiceResult<VoidDto, Exception, VoidDto>.ServerError();
        }

        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> DeleteColorAsync(IdDto id)
        {
            if (id.Id == 0)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var success = await _colorRepository.DeleteAsync(id);
            return success
                ? ServiceResult<VoidDto, Exception, VoidDto>.Ok()
                : ServiceResult<VoidDto, Exception, VoidDto>.ServerError();
        }

        public async Task<ServiceResult<ColorDto, Exception, VoidDto>> GetColorAsync()
        {
            var result = await _colorRepository.GetAllAsync();

            return result.IsSuccessful
                ? ServiceResult<ColorDto, Exception, VoidDto>.IEnumerableResult(result.Value)
                : ServiceResult<ColorDto, Exception, VoidDto>.ErrorResult(result.Error);
        }

        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> UpdateColorAsync(ColorDto colorDto)
        {
            if (colorDto == null || string.IsNullOrEmpty(colorDto.Color))
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var success = await _colorRepository.UpdateAsync(colorDto);
            return success
                ? ServiceResult<VoidDto, Exception, VoidDto>.Ok()
                : ServiceResult<VoidDto, Exception, VoidDto>.ServerError();
        }
    }
}
