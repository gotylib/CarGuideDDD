

using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Infrastructure.Services.Interfaces;

namespace CarGuideDDD.Infrastructure.Repositories.Interfaces
{
    public interface IBasketRepository
    {
        Task<bool> AddCarToBasket(AddCarToBasketDto addCarToBasketDto);
        Task<bool> UpdateCarColor( UpdateColorDto updateColorDto);

        Task<bool> DeleteCar( DeleteCarFromBasketDto deleteCarFromBasketDto);

        Task<ResultModel<EntityBasket, Exception>> GetBasket(string username); 

        Task<ResultModel<IEnumerable<EntityUser>, Exception>> GetAllBaskets();
    }
}
