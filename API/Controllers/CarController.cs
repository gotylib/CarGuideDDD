using CarGuideDDD.Infrastructure.Services;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        private readonly IStatisticsService _statisticsService;

        public CarsController(ICarService carService, IStatisticsService statisticsService)
        {
            _carService = carService;
            _statisticsService = statisticsService;
        }

        [EnableQuery]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
        [HttpGet("Get")]
        public IActionResult GetCars()
        {
            var cars = _carService.GetAllCars();

            var queryParameters = HttpContext.Request.Query;

            var queryString = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            _statisticsService.RecordVisit(baseUrl, queryString);

            return Ok(cars);

        }

        [EnableQuery]
        [HttpGet("GetFofAll")]
        public IActionResult GetForAllCars()
        {
            var cars = _carService.GetForAllCars();

            var queryParameters = HttpContext.Request.Query;

            var queryString = string.Join("&", queryParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            _statisticsService.RecordVisit(baseUrl, queryString);

            return Ok(cars);


        }


        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
        [HttpPost("CreateCar")]
        public async Task<IActionResult> CreateCar([FromBody] PriorityCarDto priorityCarDto)
        {
            await _carService.AddCarAsync(priorityCarDto);

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            await _statisticsService.RecordVisit(baseUrl, "");
            return Ok();
        }


        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Manager,Admin")]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateCar([FromBody] PriorityCarDto priorityCarDto)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            await _statisticsService.RecordVisit(baseUrl, "");
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
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            await _statisticsService.RecordVisit(baseUrl, "");

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
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            await _statisticsService.RecordVisit(baseUrl, "");

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
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            await _statisticsService.RecordVisit(baseUrl, "");

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
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

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
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

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
