using CarGuideDDD.Core.DomainObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using Riok.Mapperly.Abstractions;

namespace CarGuideDDD.Core.MapObjects
{
    [Mapper]
    public static partial class Maps
    {

        public static partial PriorityCarDto EntityCarToMapPriorityCarDto(EntityCar? entityCar);
        public static partial EntityCar MapPriorityCarDtoToEntityCar(PriorityCarDto priorityCarDto);


        public static partial EntityUser MapUserDtoToEntityUser(UserDto userDto);


        [MapProperty(nameof(EntityUser.UserName), nameof(UserDto.Username))]
        public static partial UserDto? MapEntityUseToUserDto(EntityUser? entityDto);

        public static User MapUserDtoToUser(UserDto? userDto)
        {
            var newUser = new User();
            newUser.Create(userDto?.Username ?? "default", userDto?.Email ?? "default", "", userDto?.Password ?? "default");
            return newUser;
        }
        
        public static partial User MapEntityUserToUser(EntityUser entityUser);


        [MapProperty(nameof(User.Name), nameof(UserDto.Username))]
        public static partial UserDto MapUserToUserDto(User user);

        [MapProperty(nameof(RegisterDto.Username), nameof(LoginDto.Name))]
        public static partial LoginDto MapRegisterDtoToLoginDto(RegisterDto registerDto);

        public static partial RegisterDto MapUserDtoToRegistaerDto(UserDto userDto);

        public static partial PriorityCarDto MapCarToPriorityCarDto(Car car);

        public static partial EndpointStatisticsDto MapEntityEndpointToEndpointDto(EntityEndpointStatistics entityEndpointStatistics);

        public static partial EntityEndpointStatistics MapEndpointDtoToEntityEndpoint(EndpointStatisticsDto endpointStatisticsDto);
        public static Car MapPriorityCarDtoToCar(PriorityCarDto priorityCarDto)
        {
            var newCar = new Car();
            newCar.Create(priorityCarDto.Make ?? "default", priorityCarDto.Model ?? "default", priorityCarDto.Color ?? "default", priorityCarDto.StockCount, priorityCarDto.IsAvailable);
            return newCar;

        }


    }


}
