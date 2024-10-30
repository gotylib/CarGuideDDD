using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.Token;


namespace CarGuideDDD.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private const int RefreshTokenExpirationDays = 1;

        private readonly UserManager<EntityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TokenService(UserManager<EntityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public string GenerateAccessToken(EntityUser user)
        {
            var userRoles = _userManager.GetRolesAsync(user).Result;
            
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.UserName ?? "default"),
                new Claim(ClaimTypes.Email, user.Email ?? "default")};
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = AuthOptions.GetSymmetricSecurityKey();
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                audience: AuthOptions.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshTokenDto GenerateRefreshToken()
        {
            return new RefreshTokenDto
            {
                Token = GenerateRandomToken(),
                Expiration = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)
            };
        }

        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            var user = await GetUserByRefreshTokenAsync(refreshToken);
            
            if (user == null || !IsValidRefreshToken(user, refreshToken))  throw new SecurityTokenException("Invalid refresh token");

            // Генерируем новый access token
            return GenerateAccessToken(user);
        }

        private async Task<EntityUser?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            // Получаем всех пользователей (или используем фильтрацию по refresh token в БД)
            var users = await _userManager.Users.ToListAsync();
            return users.FirstOrDefault(user => user.RefreshToken == refreshToken);
        }

        private bool IsValidRefreshToken(EntityUser user, string refreshToken)
        {
            return user.RefreshToken == refreshToken && user.RefreshTokenExpiration > DateTime.UtcNow;
        }

        public async Task<RefreshTokenDto> CreateRefreshTokenAsync(EntityUser user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiration = refreshToken.Expiration;
            // Сохраняем изменения в user
            await _userManager.UpdateAsync(user);
            return refreshToken;
        }

        private string GenerateRandomToken()
        {
            using var rng = new RNGCryptoServiceProvider();
            var randomBytes = new byte[32];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
