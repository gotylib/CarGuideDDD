using Domain.Entities;
using DTOs;
using Infrastructure.Data;
using Riok.Mapperly.Abstractions;

namespace CarGuideDDD.Core.MapObjects
{
    [Mapper]
    public static partial class Maps
    {
        public static partial PriorityCarDto EntityCarToMapPriorityCarDto(EntityCar entityCar);
        public static partial EntityCar MapPriorityCarDtoToEntityCar(PriorityCarDto priorityCarDto);


        public static partial EntityUser MapUserDtoToEntityUser(UserDto userDto);

        public static partial UserDto MapEntityUseToUserDto(EntityUser entityDto);

        [MapProperty(nameof(UserDto.Username), nameof(User.Name))]
        public static partial User MapUserDtoToUser(UserDto userDto);

        [MapProperty(nameof(User.Name), nameof(UserDto.Username))]
        public static partial UserDto MapUserDtoToUser(User user);

        [MapProperty(nameof(RegisterDto.Username), nameof(LoginDto.Name))]
        public static partial LoginDto MapRegisterDtoToLoginDto(RegisterDto registerDto);

        public static partial RegisterDto MapUserDtoToRegistaerDto(UserDto userDto);

        public static partial Car MapPriorityCarDtoToCar(PriorityCarDto priorityCarDto);

        public static partial PriorityCarDto MapCarToPriorityCarDto(Car car);

        public static partial User MapEntityUserToUser(EntityUser entityuser);

    }


}
