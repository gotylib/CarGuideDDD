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
        public async Task<ServiceResult<UserDto, Exception, VoidDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return ServiceResult<UserDto, Exception, VoidDto>.IEnumerableResult(users);
        }

        // Получение пользователя по ID
        public async Task<ServiceResult<VoidDto, Exception, UserDto>> GetUserByNameAsync(string name)
        {
            var user = await _userRepository.GetByNameAsync(name);

            if (user == null)
            {
                return ServiceResult<VoidDto, Exception, UserDto>.ErrorResult(new KeyNotFoundException($"User with ID {name} not found."));
            }

            return ServiceResult<VoidDto, Exception, UserDto>.SimpleResult(user);
        }

        // Добавление нового пользователя
        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> AddUserAsync(UserDto user)
        {
            if (user == null)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var result = await _userRepository.AddAsync(user, GenerateSecretKeyFor2FA());

            if (result.Succeeded)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.Ok();
            }

            return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
        }

        // Обновление существующего пользователя
        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> UpdateUserAsync(UserDto user)
        {
            if (user == null)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var existingUser = await _userRepository.UpdateAsync(user);

            if (existingUser.Succeeded)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.Ok();
            }

            return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
        }

        // Удаление пользователя
        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> DeleteUserAsync(string name)
        {
            var result = await _userRepository.DeleteAsync(name);

            if (result.Succeeded)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.Ok();
            }

            return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
        }

        public async Task<RegisterQrResult<VoidDto, Exception, GuidDto>> RegisterOfLogin(RegisterDto model)
        {
            if (model.Username == null)
            {
                return new RegisterQrResult<VoidDto, Exception, GuidDto>
                {
                    ActionResults = ServiceResult<VoidDto, Exception, GuidDto>.BadRequest(),
                    QrCodeStream = null
                };
            }

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                return new RegisterQrResult<VoidDto, Exception, GuidDto>
                {
                    ActionResults = await Login(Maps.MapRegisterDtoToLoginDto(model)),
                    QrCodeStream = null
                };
            }

            return await Register(model);
        }

        public async Task<RegisterQrResult<VoidDto, Exception, GuidDto>> Register(RegisterDto model)
        {
            if (model.Password == null)
            {
                return new RegisterQrResult<VoidDto, Exception, GuidDto>
                {
                    ActionResults = ServiceResult<VoidDto, Exception, GuidDto>.BadRequest(),
                    QrCodeStream = null
                };
            }

            var user = new EntityUser { UserName = model.Username, Email = model.Email };
            user.SecretCode2FA = GenerateSecretKeyFor2FA();
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new RegisterQrResult<VoidDto, Exception, GuidDto>
                {
                    ActionResults = ServiceResult<VoidDto, Exception, GuidDto>.BadRequest(),
                    QrCodeStream = null
                };
            }

            await _userManager.AddToRoleAsync(user, "User");

            if (model.SecretCode == null)
            {
                return new RegisterQrResult<VoidDto, Exception, GuidDto>
                {
                    ActionResults = null,
                    QrCodeStream = GenerateQrCode(user.SecretCode2FA, user.UserName, AuthOptions.Issuer)
                };
            }

            switch (model.SecretCode)
            {
                case "Admin":
                    await _userManager.AddToRoleAsync(user, "Admin");
                    break;
                case "Manager":
                    await _userManager.AddToRoleAsync(user, "Manager");
                    break;
            }

            return new RegisterQrResult<VoidDto, Exception, GuidDto>
            {
                ActionResults = null,
                QrCodeStream = GenerateQrCode(user.SecretCode2FA, user.UserName, AuthOptions.Issuer)
            };
        }

        public async Task<ServiceResult<VoidDto, Exception, GuidDto>> Login(LoginDto model)
        {
            if (model.Name == null)
            {
                return ServiceResult<VoidDto, Exception, GuidDto>.BadRequest();
            }

            if (model.Password == null)
            {
                return ServiceResult<VoidDto, Exception, GuidDto>.BadRequest();
            }

            var user = await _userManager.FindByNameAsync(model.Name);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ServiceResult<VoidDto, Exception, GuidDto>.BadRequest();
            }

            user.CodeFor2FA = Guid.NewGuid();
            await _userManager.UpdateAsync(user);
            return ServiceResult<VoidDto, Exception, GuidDto>.SimpleResult(new GuidDto() {Guid = user.CodeFor2FA.ToString() });
        }

        public async Task<ServiceResult<VoidDto, Exception, TokenDto>> Validate2FACode(string name, string code, string code2FA)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (code2FA != user.CodeFor2FA.ToString())
            {
                return ServiceResult<VoidDto, Exception, TokenDto>.BadRequest();
            }

            var otp = new Totp(Base32Encoding.ToBytes((await _userManager.FindByNameAsync(name)).SecretCode2FA));
            if (otp.VerifyTotp(code, out long timeStepMatched))
            {
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshToken = refreshToken.Token; // Обновление токена в базе данных
                await _userManager.UpdateAsync(user);
                return ServiceResult<VoidDto, Exception, TokenDto>.SimpleResult(new TokenDto() { AccessToken = accessToken, RefreshToken = refreshToken.Token});
            }

            return ServiceResult<VoidDto, Exception, TokenDto>.BadRequest();
        }

        public async Task<ServiceResult<VoidDto, Exception, VoidDto>> RefreshToken(RefreshTokenDto model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == model.Token);

            if (user == null || user.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return ServiceResult<VoidDto, Exception, VoidDto>.BadRequest();
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiration = newRefreshToken.Expiration;
            await _userManager.UpdateAsync(user);

            return ServiceResult<VoidDto, Exception, VoidDto>.Ok();
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
