using CarGuideDDD.Core.MapObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using Domain.Entities;
using DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace CarGuideDDD.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<EntityUser> _userManager;

        public UserRepository(UserManager<EntityUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IdentityResult> AddAsync(UserDto user)
        {

            return await _userManager.CreateAsync(Maps.MapUserDtoToEntityUser(user), user.Password);

        }

        public async Task<IdentityResult> DeleteAsync(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            return await _userManager.DeleteAsync(user);

        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return (await _userManager.Users.ToListAsync()).Select(Maps.MapEntityUseToUserDto);
        }

        public async Task<UserDto?> GetByNameAsync(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user != null)
            {
                return Maps.MapEntityUseToUserDto( user);
            }
            else
            {
                return null;
            }
        }

        public async Task<IdentityResult> UpdateAsync(UserDto user)
        {
            var Olduser = await _userManager.FindByNameAsync(user.Username);

            return await _userManager.UpdateAsync(Maps.MapUserDtoToEntityUser(user));

        }
    }
}
