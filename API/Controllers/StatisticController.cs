using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }
        [HttpGet("GetEndpointsStatistics")]
        public async Task<IActionResult> GetEndpointsStatistics()
        {
            return Ok(await _statisticsService.GetStatistics());
        }
    }
}
