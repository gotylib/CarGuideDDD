using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.Identity;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserDto?>> GetAllAsync();
        Task<UserDto?> GetByNameAsync(string name);
        Task<IdentityResult> AddAsync(UserDto user, string code);
        Task<IdentityResult> UpdateAsync(UserDto user);
        Task<IdentityResult> DeleteAsync(string name);
    }
}
