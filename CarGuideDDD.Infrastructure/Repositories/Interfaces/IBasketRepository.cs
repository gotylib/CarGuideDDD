

using CarGuideDDD.Core.DtObjects;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface IBasketRepository
    {
        Task<bool> AddCarToBasket(IdDto idDto);
        Task<bool> UpdateCarColor(
    }
}
