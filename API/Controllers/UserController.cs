﻿using CarGuideDDD.Core.DtObjects;
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
        private readonly IFileManagerService _fileManagerService;

        public UsersController(IUserService userService, IFileManagerService fileManagerService)
        {
            _userService = userService;
            _fileManagerService = fileManagerService;
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

        //[HttpPost("File")]
        //public async Task<IActionResult>

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not selected or has no content.");
            }

            using (var stream = file.OpenReadStream())
            {
                await _fileManagerService.UploadFileAsync(stream, file.FileName, Guid.NewGuid().ToString());
            }

            return Ok("File uploaded successfully.");
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            var fileNames = await _fileManagerService.ListFilesAsync();
            return Ok(fileNames);
        }
    }
}
