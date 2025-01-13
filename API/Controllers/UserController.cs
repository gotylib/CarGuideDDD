using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok( await _userService.GetAllUsersAsync());
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            return await _userService.AddUserAsync(user);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto updateUser)
        {


            return await _userService.UpdateUserAsync(updateUser);
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] UsernameDto user)
        {
            if (user.Name != null) return await _userService.DeleteUserAsync(user.Name);
            return new BadRequestObjectResult(new {massage = "Пользователь не обнаружен"});
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            return await _userService.Register(model);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            return await _userService.Login(model);
        }

        [HttpPut("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            return await _userService.RefreshToken(model);
        }


        [HttpPost("RegisterOrLogin")]
        public async Task<IActionResult> RegisterOrLogin([FromBody] UserDto userDto)
        {
            return await _userService.RegisterOfLogin(Maps.MapUserDtoToRegistaerDto(userDto));
        }

        [HttpGet("Hello")]

        public async  Task<ActionResult> Hello()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    string url = "https://mail:8085/api/Mail/Hello";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return Ok(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    return BadRequest();
                }
            }
        }
    }
}
