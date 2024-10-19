using Domain.Entities;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IUserService
    {
        // Получение всех пользователей
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        // Получение пользователя по имени
        Task<UserDto> GetUserByNameAsync(string name);

        // Добавление нового пользователя
        Task<IActionResult> AddUserAsync(UserDto user);

        // Обновление существующего пользователя
        Task<IActionResult> UpdateUserAsync(UserDto user);

        // Удаление пользователя
        Task<IActionResult> DeleteUserAsync(string name);

        Task<IActionResult> Register(RegisterDto model);
        Task<IActionResult> Login(LoginDto model);
        Task<IActionResult> RefreshToken(RefreshTokenDto model);
    }
}
