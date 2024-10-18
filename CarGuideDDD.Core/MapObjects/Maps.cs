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

    }
}
