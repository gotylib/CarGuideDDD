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


        [MapProperty(nameof(EntityUser.UserName), nameof(UserDto.Username))]
        public static partial UserDto MapEntityUseToUserDto(EntityUser entityDto);

        public static User MapUserDtoToUser(UserDto userDto)
        {
            var newUser = new User();
            newUser.Create(userDto.Username, userDto.Email, "", userDto.Password);
            return newUser;
        }

        public static partial User MapEntityUserToUser(EntityUser entityUser);


        [MapProperty(nameof(User.Name), nameof(UserDto.Username))]
        public static partial UserDto MapUserToUserDto(User user);

        [MapProperty(nameof(RegisterDto.Username), nameof(LoginDto.Name))]
        public static partial LoginDto MapRegisterDtoToLoginDto(RegisterDto registerDto);

        public static partial RegisterDto MapUserDtoToRegistaerDto(UserDto userDto);

        public static partial PriorityCarDto MapCarToPriorityCarDto(Car car);

        public static Car MapPriorityCarDtoToCar(PriorityCarDto priorityCarDto)
        {
            var newCar = new Car();
            newCar.Create(priorityCarDto.Make, priorityCarDto.Model, priorityCarDto.Color, priorityCarDto.StockCount, priorityCarDto.IsAvailable);
            return newCar;

        }



    }


}
