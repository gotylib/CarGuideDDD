using Microsoft.AspNetCore.Mvc;

using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IUserService
    {
        // Получение всех пользователей
        Task<IEnumerable<UserDto?>> GetAllUsersAsync();

        // Получение пользователя по имени
        Task<UserDto> GetUserByNameAsync(string name);

        // Добавление нового пользователя
        Task<IActionResult> AddUserAsync(UserDto user);

        // Обновление существующего пользователя
        Task<IActionResult> UpdateUserAsync(UserDto user);

        // Удаление пользователя
        Task<IActionResult> DeleteUserAsync(string name);

        Task<IActionResult> RegisterOfLogin(RegisterDto model);
        Task<RegisterQrResult> Register(RegisterDto model);
        Task<IActionResult> Login(LoginDto model);
        Task<IActionResult> RefreshToken(RefreshTokenDto model);
    }
}
