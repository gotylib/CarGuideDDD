using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
        [HttpGet("Get")]
        public async Task<IActionResult> GetCars()
        {
            var cars = await _carService.GetAllCarsAsync();
            return Ok(cars);
        }

        [HttpGet("GetFofAll")]
        public async Task<IActionResult> GetForAllCars()
        {
            var cars = await _carService.GetForAllCarsAsync();
            return Ok(cars);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
        [HttpPost("CreateCar")]
        public async Task<IActionResult> CreateCar([FromBody] PriorityCarDto priorityCarDto)
        {
            await _carService.AddCarAsync(priorityCarDto);
            return Ok();
        }


        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
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

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
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

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
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

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
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
            var username = User.Identity.Name;
            var result = await _carService.InfoAsync(carId.Id,username);
            if (result)
            {
                return Ok("Заявка сформирована");
            }
            else
            {
               return Ok("Не получилось создать заявку");
            }

        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("BuyCar")]
        public async Task<IActionResult> BuyCar([FromBody] IdDto carId)
        {
            var username = User.Identity.Name;
            var result = await _carService.BuyAsync(carId.Id, username);
            if (result)
            {
                return Ok("Заявка сформирована");
            }
            else
            {
                return Ok("Не получилось создать заявку");
            }

        }
    }
}
