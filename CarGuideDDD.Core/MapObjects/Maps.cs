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

        [MapProperty(nameof(UserDto.Username), nameof(EntityUser.UserName))]
        [MapProperty(nameof(UserDto.Email), nameof(EntityUser.Email))]
        [MapProperty(nameof(UserDto.Password), nameof(EntityUser.Password))]
        public static partial EntityUser MapUserDtoToEntityUser(UserDto userDto);


        [MapProperty(nameof(EntityUser.UserName), nameof(UserDto.Username))]
        [MapProperty(nameof(EntityUser.Email), nameof(UserDto.Email))]
        [MapProperty(nameof(EntityUser.Password),nameof(UserDto.Password))]
        public static partial UserDto MapEntityUseToUserDto (EntityUser entityDto);

        [MapProperty(nameof(PriorityCarDto.Model),nameof(Car.Model))]
        [MapProperty(nameof(PriorityCarDto.Make), nameof(Car.Make))]
        [MapProperty(nameof(PriorityCarDto.StockCount), nameof(Car.StockCount))]
        [MapProperty(nameof(PriorityCarDto.Color), nameof(Car.Color))]
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
