using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;
using Microsoft.IdentityModel.Tokens;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using System.Security.Claims;
using CarGuideDDD.Infrastructure.Services;
using API.ODataSettings;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IColorService _colorService;
        private readonly IBasketService _basketService;


        public CarsController(ICarService carService, IFileManagerService fileManagerService, IColorService colorService, IBasketService basketService)
        {
            _carService = carService;
            _fileManagerService = fileManagerService;
            _colorService = colorService;
            _basketService = basketService;
        }

        [EnableQuery]
        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet("Get")]
        public IActionResult GetCars()
        {
            return Ok(_carService.GetAllCars());
        }

        [EnableQuery]
        [DisableFilter("AddUserName")]
        [Authorize(Policy = "All")]
        [HttpGet("GetForAll")]
        public IActionResult GetForAll()
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
                // Получение утверждений текущего пользователя
                var claims = HttpContext.User.Claims;

                // Извлечение ролей из утверждений
                var roles = claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList(); 
                var result = await _carService.AddCarAsync(priorityCarDto, roles);
                if (result.Success)
                {
                    return result.Message.IsNullOrEmpty()
                        ? Ok()
                        : Ok(result.Message);
                }
                if (result.StatusCode >= 400 && result.StatusCode < 500)
                {
                    return result.Message.IsNullOrEmpty()
                        ? BadRequest()
                        : BadRequest(result.Message);
                }
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
                // Получение утверждений текущего пользователя
                var claims = HttpContext.User.Claims;

                // Извлечение ролей из утверждений
                var roles = claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var result = await _carService.UpdateCarAsync(priorityCarDto.Id, priorityCarDto, roles);
                if (result.Success)
                {
                    return result.Message.IsNullOrEmpty()
                        ? Ok()
                        : Ok(result.Message);
                }
                if (result.StatusCode >= 400 && result.StatusCode < 500)
                {
                    return result.Message.IsNullOrEmpty()
                        ? BadRequest()
                        : BadRequest(result.Message);
                }
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
                // Получение утверждений текущего пользователя
                var claims = HttpContext.User.Claims;

                // Извлечение ролей из утверждений
                var roles = claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var result = await _carService.DeleteCarAsync(idDte.Id, roles);
                if (result.Success)
                {
                    return result.Message.IsNullOrEmpty()
                        ? Ok()
                        : Ok(result.Message);
                }
                if (result.StatusCode >= 400 && result.StatusCode < 500)
                {
                    return result.Message.IsNullOrEmpty()
                        ? BadRequest()
                        : BadRequest(result.Message);
                }

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
                if (result.Success)
                {
                    return result.Message.IsNullOrEmpty()
                        ? Ok()
                        : Ok(result.Message);
                }
                if (result.StatusCode >= 400 && result.StatusCode < 500)
                {
                    return result.Message.IsNullOrEmpty()
                        ? BadRequest()
                        : BadRequest(result.Message);
                }

                return StatusCode(result.StatusCode);
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
            var result =  await _colorService.AddColorAsync(colorDto);
            if (result.Success)
            {
                return result.Message.IsNullOrEmpty()
                    ? Ok()
                    : Ok(result.Message);
            }
            if(result.StatusCode >= 400 && result.StatusCode < 500)
            {
                return result.Message.IsNullOrEmpty()
                    ? BadRequest()
                    : BadRequest(result.Message);
            }

            return StatusCode(result.StatusCode);

        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("DeleteColor")]
        public async Task<IActionResult> DeleteColor(IdDto idDto)
        {
            var result = await _colorService.DeleteColorAsync(idDto);
            if (result.Success)
            {
                return result.Message.IsNullOrEmpty()
                    ? Ok()
                    : Ok(result.Message);
            }
            if (result.StatusCode >= 400 && result.StatusCode < 500)
            {
                return result.Message.IsNullOrEmpty()
                    ? BadRequest()
                    : BadRequest(result.Message);
            }

            return StatusCode(result.StatusCode);
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("UpdateColor")]
        public async Task<IActionResult> UpdateColor(ColorDto colorDto)
        {
            var result = await _colorService.UpdateColorAsync(colorDto);
            if (result.Success)
            {
                return result.Message.IsNullOrEmpty()
                    ? Ok()
                    : Ok(result.Message);
            }
            if (result.StatusCode >= 400 && result.StatusCode < 500)
            {
                return result.Message.IsNullOrEmpty()
                    ? BadRequest()
                    : BadRequest(result.Message);
            }

            return StatusCode(result.StatusCode);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("GetColors")]
        public async Task<IActionResult> GetColors()
        {
            var result = await _colorService.GetColorAsync();

            if (result.Success)
            {
                if(result.Results == null)
                {
                    return Ok(result.Result);
                }
                return Ok(result.Results);
            }
            if(result.Error != null)
            {
                return BadRequest(result.Error);
            }
            return StatusCode(result.StatusCode);
        }

        [Authorize(Policy = "UserOrAdmin")]
        [HttpPost("AddCarToBasket")]
        public async Task<IActionResult> AddCarToBasket([FromBody] AddCarToBasketDto addCarToBasketDto)
        {
            // Получение утверждений текущего пользователя
            var claims = HttpContext.User.Claims;

            // Извлечение ролей из утверждений
            var roles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Извлечение имени пользователя из утверждений
            var username = claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var result = await _basketService.AddCarToBasket(addCarToBasketDto, roles, username);
            if (result.Success)
            {
                return result.Message.IsNullOrEmpty()
                    ? Ok()
                    : Ok(result.Message);
            }
            if (result.StatusCode >= 400 && result.StatusCode < 500)
            {
                return result.Message.IsNullOrEmpty()
                    ? BadRequest()
                    : BadRequest(result.Message);
            }

            return StatusCode(result.StatusCode);
        }

        [Authorize(Policy = "UserOrAdmin")]
        [HttpDelete("DeleteCarFromBasket")]
        public async Task<IActionResult> DeleteCarFromBasket([FromBody] DeleteCarFromBasketDto deleteCarFromBasketDto)
        {
            // Получение утверждений текущего пользователя
            var claims = HttpContext.User.Claims;

            // Извлечение ролей из утверждений
            var roles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Извлечение имени пользователя из утверждений
            var username = claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var result = await _basketService.DeleteCarFromBasket(deleteCarFromBasketDto, roles, username);
            if (result.Success)
            {
                return result.Message.IsNullOrEmpty()
                    ? Ok()
                    : Ok(result.Message);
            }
            if (result.StatusCode >= 400 && result.StatusCode < 500)
            {
                return result.Message.IsNullOrEmpty()
                    ? BadRequest()
                    : BadRequest(result.Message);
            }

            return StatusCode(result.StatusCode);
        }

        [Authorize(Policy = "UserOrAdmin")]
        [HttpGet("GetBasket")]
        public async Task<IActionResult> GetBasket()
        {
            // Получение утверждений текущего пользователя
            var claims = HttpContext.User.Claims;

            // Извлечение ролей из утверждений
            var roles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Извлечение имени пользователя из утверждений
            var username = claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var result = await _basketService.GetCarFromBasker(roles, username);
            if (result.Success)
            {
                if (result.Results == null)
                {
                    return Ok(result.Result);
                }
                return Ok(result.Results);
            }
            if (result.Error != null)
            {
                return BadRequest(result.Error);
            }
            return StatusCode(result.StatusCode);
        }

        [Authorize(Policy = "UserOrAdmin")]
        [HttpPut("UpdateColorToCarFromBasket")]
        public async Task<IActionResult> UpdateColorToCarFromBasket([FromBody] UpdateColorDto updateColorDto)
        {
            // Получение утверждений текущего пользователя
            var claims = HttpContext.User.Claims;

            // Извлечение ролей из утверждений
            var roles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Извлечение имени пользователя из утверждений
            var username = claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var result = await _basketService.UpdateColorToCarFromBasket(updateColorDto, roles, username);
            if (result.Success)
            {
                return result.Message.IsNullOrEmpty()
                    ? Ok()
                    : Ok(result.Message);
            }
            if (result.StatusCode >= 400 && result.StatusCode < 500)
            {
                return result.Message.IsNullOrEmpty()
                    ? BadRequest()
                    : BadRequest(result.Message);
            }

            return StatusCode(result.StatusCode);
        }

    }
}
