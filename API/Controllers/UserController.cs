using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Services;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IStatisticsService _statisticsService;

        public UsersController(IUserService userService, IStatisticsService statisticsService)
        {
            _userService = userService;
            _statisticsService = statisticsService;
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.AddUserAsync(user);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto updateUser)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.UpdateUserAsync(updateUser);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] UsernameDto user)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.DeleteUserAsync(user.Name);

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.Register(model);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.Login(model);
        }

        [HttpPut("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.RefreshToken(model);
        }


        [HttpPost("RegisterOrLogin")]
        public async Task<IActionResult> RegisterOrLogin([FromBody] UserDto userDto)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            await _statisticsService.RecordVisit(baseUrl, "");

            return await _userService.RegisterOfLogin(Maps.MapUserDtoToRegistaerDto(userDto));
        }
    }
}
