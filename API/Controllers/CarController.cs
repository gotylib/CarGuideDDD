﻿using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
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

        [EnableQuery]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
        [HttpGet("Get")]
        public IActionResult GetCars()
        {
            return Ok(_carService.GetAllCars());
        }

        [EnableQuery]
        [HttpGet("GetFofAll")]
        public IActionResult GetForAllCars()
        {
            return Ok(_carService.GetForAllCars());
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
    }
}
