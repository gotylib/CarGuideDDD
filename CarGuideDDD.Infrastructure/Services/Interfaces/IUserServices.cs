using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.AnswerObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IUserService
    {
        // Получение всех пользователей
        Task<ServiceResult<UserDto, Exception, VoidDto>> GetAllUsersAsync();

        // Получение пользователя по имени
        Task<ServiceResult<VoidDto, Exception, UserDto>> GetUserByNameAsync(string name);

        // Добавление нового пользователя
        Task<ServiceResult<VoidDto, Exception, VoidDto>> AddUserAsync(UserDto user);

        // Обновление существующего пользователя
        Task<ServiceResult<VoidDto, Exception, VoidDto>> UpdateUserAsync(UserDto user);

        // Удаление пользователя
        Task<ServiceResult<VoidDto, Exception, VoidDto>> DeleteUserAsync(string name);

        // Регистрация или вход
        Task<RegisterQrResult<VoidDto, Exception, GuidDto>> RegisterOfLogin(RegisterDto model);

        // Регистрация
        Task<RegisterQrResult<VoidDto, Exception, GuidDto>> Register(RegisterDto model);

        // Вход
        Task<ServiceResult<VoidDto, Exception, GuidDto>> Login(LoginDto model);

        // Обновление токена
        Task<ServiceResult<VoidDto, Exception, VoidDto>> RefreshToken(RefreshTokenDto model);

        // Валидация 2FA кода
        Task<ServiceResult<VoidDto, Exception, TokenDto>> Validate2FACode(string name, string code, string code2FA);
    }

}
