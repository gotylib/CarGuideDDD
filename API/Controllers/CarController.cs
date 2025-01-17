﻿using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;
using CarGuideDDD.Infrastructure.Services.Hosted_Services;
using Microsoft.IdentityModel.Tokens;
using CarGuideDDD.Infrastructure.Services;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using CarGuideDDD.Core.EntityObjects;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IColorService _colorService;


        public CarsController(ICarService carService, IFileManagerService fileManagerService, IColorService colorService)
        {
            _carService = carService;
            _fileManagerService = fileManagerService;
            _colorService = colorService;
        }

        [EnableQuery]
        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet("Get")]
        public IActionResult GetCars()
        {
            return Ok(_carService.GetAllCars());
        }

        [EnableQuery]
        [Authorize]
        [HttpGet("GetFofAll")]
        public IActionResult GetForAllCars()
        {
            return Ok(_carService.GetForAllCars());
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpPost("CreateCar")]
        public async Task<IActionResult> CreateCar([FromBody] PriorityCarDto priorityCarDto)
        {
            if (priorityCarDto.Make.IsNullOrEmpty() || priorityCarDto.Model.IsNullOrEmpty() || priorityCarDto.Color.IsNullOrEmpty())
            {
                return BadRequest();
            }
            priorityCarDto.AddTime = (DateTime.Now).ToUniversalTime();
            var username = User.Identity?.Name;
            if (username == null)
            {
                return BadRequest("Проблемы с нахождением пользователя");
            }
            priorityCarDto.AddUserName = username;
            priorityCarDto.NameOfPhoto = "";
            try
            {
                await _carService.AddCarAsync(priorityCarDto);
            }
            catch(ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            
            return Ok();
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpPost("AddCarPhoto")]
        public async Task<IActionResult> AddCarPhoto([FromForm] CarPhotoDto carPhotoDto)
        {
            if (carPhotoDto.file == null || carPhotoDto.file.Length == 0 || carPhotoDto.Make.IsNullOrEmpty() || carPhotoDto.Model.IsNullOrEmpty() || carPhotoDto.Color.IsNullOrEmpty())
            {
                return BadRequest("Don't have information");
            }

            using var stream = carPhotoDto.file.OpenReadStream();
            var guid = $"{Guid.NewGuid()}.{carPhotoDto.file.FileName.Split('.')[1]}";
            await _fileManagerService.UploadFileAsync(stream, carPhotoDto.file.FileName, guid);
            try
            {
                await _carService.AddPhotoToCarAsync(carPhotoDto, guid);
            }
            catch(ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("File uploaded successfully.");
        }

        [HttpPost("GetCarPhoto")]
        public async Task<IActionResult> GetCarPhoto(string name)
        {
            // Получаем IFormFile
            var file = await _fileManagerService.GetFileAsync(name);

            if (file == null)
            {
                return NotFound(); // Если файл не найден
            }

            // Возвращаем файл в ответе
            return File(file.OpenReadStream(), file.ContentType, file.FileName);
        }


        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateCar([FromBody] PriorityCarDto priorityCarDto)
        {

            try
            {
                await _carService.UpdateCarAsync(priorityCarDto.Id, priorityCarDto);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteCar([FromBody] IdDto idDte)
        {
            try
            {
                await _carService.DeleteCarAsync(idDte.Id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpPut("Quantity")]
        public async Task<IActionResult> UpdateCarQuantity([FromBody] QuantityDto quantityDto)
        {
            try
            {
                await _carService.UpdateCarQuantityAsync(quantityDto.Id, quantityDto.Quantity);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpPut("Availability")]
        public async Task<IActionResult> SetCarAvailability([FromBody] IsAvailableDto isAvailableDto)
        {
            try
            {
                var result = await _carService.SetCarAvailabilityAsync(isAvailableDto.Id, isAvailableDto.IsAvailable);
                return result;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("InformateCar")]
        public async Task<IActionResult> InformateCar([FromBody] IdDto carId)
        {

            var username = User.Identity?.Name;

            if(username == null)
            {
                return BadRequest("Проблемы с нахождением пользователя");
            } 

            var result = await _carService.InfoAsync(carId.Id,username);
            return Ok(result ? "Заявка сформирована" : "Не получилось создать заявку");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("BuyCar")]
        public async Task<IActionResult> BuyCar([FromBody] IdDto carId)
        {
            var username = User.Identity.Name;
            var result = await _carService.BuyAsync(carId.Id, username);
            return Ok(result ? "Заявка сформирована" : "Не получилось создать заявку");
        }



        [Authorize(Policy = "Admin")]
        [HttpPost("AddColor")]
        public async Task<IActionResult> AddColor(ColorDto colorDto)
        {
            return await _colorService.AddColorAsync(colorDto);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("DeleteColor")]
        public async Task<IActionResult> DeleteColor(IdDto idDto)
        {
            return await _colorService.DeleteColorAsync(idDto);
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("UpdateColor")]
        public async Task<IActionResult> UpdateColor(ColorDto colorDto)
        {
            return await _colorService.UpdateColorAsync(colorDto);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("GetColors")]
        public async Task<IActionResult> GetColors()
        {
            return await _colorService.GetColorAsync();
        }
    }
}
