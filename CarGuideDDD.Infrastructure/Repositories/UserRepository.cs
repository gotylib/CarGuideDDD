using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.MapObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;



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
            return await _userManager.CreateAsync(Maps.MapUserDtoToEntityUser(user), user.Password ?? "default");
        }

        public async Task<IdentityResult> DeleteAsync(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user != null)
            {
                return await _userManager.DeleteAsync(user);
            }
            
            return new IdentityResult();
            
        }

        public async Task<IEnumerable<UserDto?>> GetAllAsync()
        {
            return (await _userManager.Users.ToListAsync())
                .Select(Maps.MapEntityUseToUserDto);

        }


        public async Task<UserDto?> GetByNameAsync(string name)
        {
                return Maps.MapEntityUseToUserDto(await _userManager.FindByNameAsync(name));
        }

        public async Task<IdentityResult> UpdateAsync(UserDto user)
        {
            if(user.Username != null)
            {
                return await _userManager.UpdateAsync(Maps.MapUserDtoToEntityUser(user));
            }

            return new IdentityResult();

        }
    }
}
