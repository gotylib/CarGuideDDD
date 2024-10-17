using Domain.Entities;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        // Получение всех пользователей
        Task<List<User>> GetAllUsersAsync();

        // Получение пользователя по имени
        Task<User> GetUserByNameAsync(string name);

        // Добавление нового пользователя
        Task<IActionResult> AddUserAsync(User user);

        // Обновление существующего пользователя
        Task<IActionResult> UpdateUserAsync(User user);

        // Удаление пользователя
        Task<IActionResult> DeleteUserAsync(string name);

        Task<IActionResult> Register(RegisterDto model);
        Task<IActionResult> Login(LoginDto model);
        Task<IActionResult> RefreshToken(RefreshTokenDto model);
    }
}
