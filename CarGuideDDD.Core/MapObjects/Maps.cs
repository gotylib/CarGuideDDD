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

        public static partial UserDto MapEntityUseToUserDto (EntityUser entityDto);

        public static partial Car MapPriorityCarDtoToCar(PriorityCarDto priorityCarDto);

        [MapProperty(nameof(UserDto.Username), nameof(User.Name))]
        [MapProperty(nameof(UserDto.Email), nameof(User.Email))]
        public static partial User MapUserDtoToUser(UserDto userDto);

        [MapProperty(nameof(RegisterDto.Username), nameof(LoginDto.Name))]
        [MapProperty(nameof(RegisterDto.Password), nameof(LoginDto.Password))]
        public static partial LoginDto MapRegisterDtoToLoginDto(RegisterDto registerDto);

        public static partial RegisterDto MapUserDtoToRegistaerDto(UserDto userDto);
    }


}
