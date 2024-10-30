using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.MapObjects;
using Microsoft.AspNetCore.Identity;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;



namespace CarGuideDDD.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly UserManager<EntityUser> _userManager;

        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, UserManager<EntityUser> userManager, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        // Получение всех пользователей
        public async Task<IEnumerable<UserDto?>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        // Получение пользователя по ID
        public async Task<UserDto> GetUserByNameAsync(string name)
        {
            var user = await _userRepository.GetByNameAsync(name);
            
            if (user == null) throw new KeyNotFoundException($"User with ID {name} not found.");

            return user;
        }

        // Добавление нового пользователя
        public async Task<IActionResult> AddUserAsync(UserDto user)
        {
            if (user == null)throw new ArgumentNullException(nameof(user), "User cannot be null.");
            
            var result = await _userRepository.AddAsync(user);
            
            if (result.Succeeded) return new OkResult();
            
            return new BadRequestObjectResult(result.Errors);

        }

        // Обновление существующего пользователя
        public async Task<IActionResult> UpdateUserAsync(UserDto user)
        {
            if (user == null)throw new ArgumentNullException(nameof(user), "User cannot be null.");
            
            var existingUser = await _userRepository.UpdateAsync(user);
            
            if (existingUser.Succeeded) return new OkResult();
            
            return new BadRequestObjectResult(existingUser.Errors);
            
        }

        // Удаление пользователя
        public async Task<IActionResult> DeleteUserAsync(string name)
        {
            var result = await _userRepository.DeleteAsync(name);
            
            if (result.Succeeded) return new OkResult();

            return new BadRequestObjectResult(result.Errors);
        }

        public async Task<IActionResult> RegisterOfLogin(RegisterDto model)
        {
            if (model.Username == null) return new BadRequestResult();
            
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user != null) return await Login(Maps.MapRegisterDtoToLoginDto(model));
            
            await Register(model);
            
            return await Login(Maps.MapRegisterDtoToLoginDto(model));

        }

        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (model.Password == null) return new BadRequestObjectResult(new { massege = "Пароль не указан" });
            var user = new EntityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(result.Errors);
            await _userManager.AddToRoleAsync(user, "User");

            if (model.SecretCode == null) return new OkResult();
            switch (model.SecretCode)
            {
                case "Admin":
                    await _userManager.AddToRoleAsync(user, "Admin");
                    break;
                case "Manager":
                    await _userManager.AddToRoleAsync(user, "Manager");
                    break;
            }
            return new OkResult();

        }

        public async Task<IActionResult> Login(LoginDto model)
        {
            if (model.Name == null)  return new BadRequestObjectResult(new { message = "Имя не указано." });

            if (model.Password == null) return new BadRequestObjectResult(new {message = "Пароль не указан."});
            
            var user = await _userManager.FindByNameAsync(model.Name);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return new UnauthorizedResult();
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken.Token; // Обновление токена в базе данных
            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new { AccessToken = accessToken, RefreshToken = refreshToken.Token });
        }

        public async Task<IActionResult> RefreshToken(RefreshTokenDto model)
        {
            if (string.IsNullOrEmpty(model.Token)) return new BadRequestObjectResult("Refresh token is required.");


            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == model.Token);

            if (user == null || user.RefreshTokenExpiration < DateTime.UtcNow) return new UnauthorizedResult();
            
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiration = newRefreshToken.Expiration;
            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token });
        }



    }
}
