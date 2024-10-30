
using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IMailServices
    {
        public bool SendUserNotFountManagerMessage(UserDto user);

        public bool SendUserNoHaveCarMessage(UserDto user, PriorityCarDto car);

        public bool SendBuyCarMessage(UserDto user, UserDto? manager, PriorityCarDto car);

        public bool SendInformCarMessage(UserDto user, UserDto manager, PriorityCarDto car);

    }
}
