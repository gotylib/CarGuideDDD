
using DTOs;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IMailServices
    {
        public Task<bool> SendUserNotFountManagerMessageAsync(UserDto user);

        public Task<bool> SendUserNoHaweCarMessageAsync(UserDto user, PriorityCarDto car);

        public Task<bool> SendBuyCarMessageAsync(UserDto user, UserDto manager, PriorityCarDto car);

        public Task<bool> SendInformCarMessageAsync(UserDto user, UserDto manager, PriorityCarDto car);

    }
}
