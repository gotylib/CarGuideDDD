using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Services;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IKeycloakAdminClientService _keycloakAdminClientService;

        public UsersController(IUserService userService, IFileManagerService fileManagerService, IKeycloakAdminClientService keycloakAdminClientService)
        {
            _userService = userService;
            _fileManagerService = fileManagerService;
            _keycloakAdminClientService = keycloakAdminClientService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok( await _userService.GetAllUsersAsync());
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            var result = await _userService.AddUserAsync(user);
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
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto updateUser)
        {

            var result = await _userService.UpdateUserAsync(updateUser);
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
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] UsernameDto user)
        {
            if (user.Name != null) 
            {
                var result = await _userService.DeleteUserAsync(user.Name);
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
            return BadRequest("Пользователь не обнаружен");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _userService.Register(model);
            if(result.QrCodeStream == null)
            {
                var actionResult =  result.ActionResults;
                if (actionResult.Success)
                {
                    return actionResult.Message.IsNullOrEmpty()
                        ? Ok()
                        : Ok(actionResult.Message);
                }
                if (actionResult.StatusCode >= 400 && actionResult.StatusCode < 500)
                {
                    return actionResult.Message.IsNullOrEmpty()
                        ? BadRequest()
                        : BadRequest(actionResult.Message);
                }

                return StatusCode(actionResult.StatusCode);
            }
            return File(result.QrCodeStream.ToArray(), "image/png", "qrcode.png");
            
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _userService.Login(model);
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

        [HttpPut("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            var result = await _userService.RefreshToken(model);
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


        [HttpPost("RegisterOrLogin")]
        public async Task<IActionResult> RegisterOrLogin([FromBody] UserDto userDto)
        {
            var result = await _userService.RegisterOfLogin(Maps.MapUserDtoToRegistaerDto(userDto));
            if (result.QrCodeStream == null)
            {
                var actionResult = result.ActionResults;
                if (actionResult.Success)
                {
                    return actionResult.Message.IsNullOrEmpty()
                        ? Ok()
                        : Ok(actionResult.Message);
                }
                if (actionResult.StatusCode >= 400 && actionResult.StatusCode < 500)
                {
                    return actionResult.Message.IsNullOrEmpty()
                        ? BadRequest()
                        : BadRequest(actionResult.Message);
                }

                return StatusCode(actionResult.StatusCode);
            }
            return File(result.QrCodeStream.ToArray(), "image/png", "qrcode.png");
        }

        [HttpGet("Auth2FA")]
        public async Task<IActionResult> Auth2FA(string code, string code2FA,string name)
        {

            var result = await _userService.Validate2FACode(name, code, code2FA);

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

        [Authorize(AuthenticationSchemes = "Keycloak")]
        [HttpPost("StateRole")]
        public async Task<IActionResult> StateRole(string secret)
        {
            // Извлечение JWT из заголовка авторизации
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized();
            }

            var token = authorizationHeader.Substring(7); // Удаление "Bearer "

            // Декодирование JWT и извлечение информации
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Username not found in JWT");
            }

            // Получение userid по имени пользователя
            //var userId = await _keycloakAdminClientService.GetUserIdByUsernameAsync(username);

            // Добавление роли пользователю
            var roleName = "Admin"; // Имя роли, которую вы хотите добавить
            await _keycloakAdminClientService.AddRoleToUserAsync(userId, roleName);

            return Ok("Role added successfully");
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            var fileNames = await _fileManagerService.ListFilesAsync();
            return Ok(fileNames);
        }
    }
}
