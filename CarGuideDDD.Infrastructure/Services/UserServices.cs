using Microsoft.AspNetCore.Mvc;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using Microsoft.AspNetCore.Identity;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using OtpNet;
using CommunityToolkit.HighPerformance.Helpers;
using QRCoder;
using CarGuideDDD.Core.Token;
using CarGuideDDD.Core.AnswerObjects;




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
        public async Task<ServiceResult> AddUserAsync(UserDto user)
        {
            if (user == null)throw new ArgumentNullException(nameof(user), "User cannot be null.");
            
            var result = await _userRepository.AddAsync(user, GenerateSecretKeyFor2FA());

            
            if (result.Succeeded) return ServiceResult.Ok();
            
            return ServiceResult.BadRequest(result.Errors.ToString());

        }

        // Обновление существующего пользователя
        public async Task<ServiceResult> UpdateUserAsync(UserDto user)
        {
            if (user == null)throw new ArgumentNullException(nameof(user), "User cannot be null.");
            
            var existingUser = await _userRepository.UpdateAsync(user);
            
            if (existingUser.Succeeded) return ServiceResult.Ok();
            
            return ServiceResult.BadRequest(existingUser.Errors.ToString());
            
        }

        // Удаление пользователя
        public async Task<ServiceResult> DeleteUserAsync(string name)
        {
            var result = await _userRepository.DeleteAsync(name);
            
            if (result.Succeeded) return ServiceResult.Ok();

            return ServiceResult.BadRequest(result.Errors.ToString());
        }

        public async Task<RegisterQrResult> RegisterOfLogin(RegisterDto model)
        {
            if (model.Username == null) return new RegisterQrResult(){ ActionResults = ServiceResult.BadRequest("User is null"), QrCodeStream = null};
            
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null) return new RegisterQrResult() { ActionResults = await Login(Maps.MapRegisterDtoToLoginDto(model)), QrCodeStream = null };
            
            return await Register(model);

        }

        public async Task<RegisterQrResult> Register(RegisterDto model)
        {
            if (model.Password == null) return new RegisterQrResult() {ActionResults = ServiceResult.BadRequest( "Пароль не указан" ), QrCodeStream = null};
            var user = new EntityUser { UserName = model.Username, Email = model.Email };
            user.SecretCode2FA = GenerateSecretKeyFor2FA();
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return new RegisterQrResult() { ActionResults = ServiceResult.BadRequest(result.Errors.ToString()), QrCodeStream = null };
            await _userManager.AddToRoleAsync(user, "User");

            if (model.SecretCode == null) return new RegisterQrResult() { ActionResults = null, QrCodeStream = GenerateQrCode(user.SecretCode2FA, user.UserName, AuthOptions.Issuer) };
            switch (model.SecretCode)
            {
                case "Admin":
                    await _userManager.AddToRoleAsync(user, "Admin");
                    break;
                case "Manager":
                    await _userManager.AddToRoleAsync(user, "Manager");
                    break;
            }

            return new RegisterQrResult() { ActionResults = null, QrCodeStream = GenerateQrCode(user.SecretCode2FA, user.UserName, AuthOptions.Issuer) };

        }

        public async Task<ServiceResult> Login(LoginDto model)
        {
            if (model.Name == null)  return ServiceResult.BadRequest( "Имя не указано.");

            if (model.Password == null) return ServiceResult.BadRequest("Пароль не указан.");
            
            var user = await _userManager.FindByNameAsync(model.Name);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ServiceResult.BadRequest("Not Authorize",401);
            }

            user.CodeFor2FA = Guid.NewGuid();
            await _userManager.UpdateAsync(user);
            return ServiceResult.Ok(user.CodeFor2FA.ToString());
        }

        public async Task<ServiceResult> Validate2FACode(string name, string code, string code2FA)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (code2FA != user.CodeFor2FA.ToString()) return ServiceResult.BadRequest("Code not valid");
 
            var otp = new Totp(Base32Encoding.ToBytes((await _userManager.FindByNameAsync(name)).SecretCode2FA));
            if(otp.VerifyTotp(code, out long timeStepMatched))
            {
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshToken = refreshToken.Token; // Обновление токена в базе данных
                await _userManager.UpdateAsync(user);
                return ServiceResult.Ok($"AccessToken = {accessToken}, RefreshToken = {refreshToken.Token}");
            }
            return ServiceResult.BadRequest("Code not valid");
        }
        public async Task<ServiceResult> RefreshToken(RefreshTokenDto model)
        {
            if (string.IsNullOrEmpty(model.Token)) return ServiceResult.BadRequest("Refresh token is required.");


            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == model.Token);

            if (user == null || user.RefreshTokenExpiration < DateTime.UtcNow) return ServiceResult.BadRequest("Not Authorize", 401);
            
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiration = newRefreshToken.Expiration;
            await _userManager.UpdateAsync(user);

            return ServiceResult.Ok($"AccessToken = {newAccessToken}, RefreshToken = {newRefreshToken.Token} ");
        }

        public MemoryStream GenerateQrCode(string secretKey, string username, string issuer)
        {
            string uri = $"otpauth://totp/{issuer}:{username}?secret={secretKey}&issuer={issuer}";
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return new MemoryStream(qrCodeImage);

        }

        private string GenerateSecretKeyFor2FA()
        {
            var key = KeyGeneration.GenerateRandomKey(20);

            return Base32Encoding.ToString(key);
            
        }


    }
}
