using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;


namespace CarGuideDDD.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<EntityUser> _userManager;
        private readonly RoleManager<EntityUser> _roleManager;

        public UserRepository(UserManager<EntityUser> userManager, RoleManager<EntityUser> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IdentityResult> AddAsync(User user)
        {

            return await _userManager.CreateAsync(EntityUser.ConwertToEntityUser(user), user.Password);

        }

        public async Task<IdentityResult> DeleteAsync(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            return await _userManager.DeleteAsync(user);

        }

        public async Task<List<EntityUser>> GetAllAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<EntityUser?> GetByNameAsync(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<IdentityResult> UpdateAsync(User user)
        {
            var Olduser = await _userManager.FindByNameAsync(user.Name);

            return await _userManager.UpdateAsync(EntityUser.ConwertToEntityUser(user));

        }
    }
}
