

namespace CarGuideDDD.Infrastructure.Services.Сhecks
{
    public static class RolesCheck
    {
        public static bool IsAdmin(List<string> roles)
        {
            return roles.Contains("Admin")
                ? true 
                : false;
        }

        public static bool IsManager(List<string> roles)
        {
            return roles.Contains("Manager")
                ? true 
                : false;
        }

        public static bool IsUser(List<string> roles)
        {
            return roles.Contains("User")
                ? true
                : false;
        }
    }
}
