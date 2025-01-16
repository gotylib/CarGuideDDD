
namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IKeycloakAdminClientService
    {
        public Task<string> GetAccessTokenAsync();
        public Task<string> GetUserIdByUsernameAsync(string username);
        public Task AddRoleToUserAsync(string userId, string roleName);
    }
}
