using Domain.Entities;
using Domain.Repositories;
using Domain.Services.Interfaces;
using DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;


namespace Domain.Services
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
        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();
            var entityUsers =  await _userRepository.GetAllAsync();

            for(int i = 0; i < entityUsers.Count; ++i)
            {
                users.Add(new User(entityUsers[i].UserName, entityUsers[i].Email, entityUsers[i].PasswordHash));
            }
            return users;
        }

        // Получение пользователя по ID
        public async Task<User> GetUserByNameAsync(string name)
        {
            var user = EntityUser.ConvertToUser( (await _userRepository.GetByNameAsync(name)));
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {name} not found.");
            }
            return user;
        }

        // Добавление нового пользователя
        public async Task<IActionResult> AddUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var result = await _userRepository.AddAsync(user);
            if (result.Succeeded)
            {
                return new OkResult();
            }
            else
            {
                return new BadRequestObjectResult(result.Errors);
            }
        }

        // Обновление существующего пользователя
        public async Task<IActionResult> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var existingUser = await _userRepository.UpdateAsync(user);
            if (existingUser.Succeeded)
            {
                return new OkResult();
            }
            else
            {
                return new BadRequestObjectResult(existingUser.Errors);
            }
        }

        // Удаление пользователя
        public async Task<IActionResult> DeleteUserAsync(string name)
        {
           var result =  await _userRepository.DeleteAsync(name);
            if (result.Succeeded)
            {
                return new OkResult();
            }
            else
            {
                return new BadRequestObjectResult(result.Errors);
            }
        }

        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new EntityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                if (model.SecretCode != null)
                {
                    if (model.SecretCode == "Admin")
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else if (model.SecretCode == "Manager")
                    {
                        await _userManager.AddToRoleAsync(user, "Manager");
                    }
                }

                return new OkResult();
            }

            return new BadRequestObjectResult(result.Errors);
        }

        public async Task<IActionResult> Login(LoginDto model)
        {
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
            if (string.IsNullOrEmpty(model.Token))
            {
                return new BadRequestObjectResult("Refresh token is required.");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == model.Token);

            if (user == null || user.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return new UnauthorizedResult();
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken.Token;
            user.RefreshTokenExpiration = newRefreshToken.Expiration;
            await _userManager.UpdateAsync(user);

            return new OkObjectResult(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token });
        }

    }

}
