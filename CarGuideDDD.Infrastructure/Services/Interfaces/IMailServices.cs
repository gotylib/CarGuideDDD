
namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IMailServices
    {
        public Task<bool> SendUserNotFountManagerMessageAsync(string email);

        public Task<bool> SendBuyCarMessageAsync(string clientEmail, string managerEmail);

        public Task<bool> SendInformCarMessageAsync(string clientEmail, string managerEmail);

    }
}
