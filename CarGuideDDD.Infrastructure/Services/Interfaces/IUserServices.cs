using Microsoft.AspNetCore.Mvc;

using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.AnswerObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IUserService
    {
        // Получение всех пользователей
        Task<IEnumerable<UserDto?>> GetAllUsersAsync();

        // Получение пользователя по имени
        Task<UserDto> GetUserByNameAsync(string name);

        // Добавление нового пользователя
        Task<ServiceResult> AddUserAsync(UserDto user);

        // Обновление существующего пользователя
        Task<ServiceResult> UpdateUserAsync(UserDto user);

        // Удаление пользователя
        Task<ServiceResult> DeleteUserAsync(string name);

        Task<RegisterQrResult> RegisterOfLogin(RegisterDto model);
        Task<RegisterQrResult> Register(RegisterDto model);
        Task<ServiceResult> Login(LoginDto model);
        Task<ServiceResult> RefreshToken(RefreshTokenDto model);
        Task<ServiceResult> Validate2FACode(string name, string code, string code2FA);
    }
}
