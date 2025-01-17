using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Infrastructure.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _repository;
        public BasketService(IBasketRepository repository)
        {
            _repository = repository;
        }
        public Task<IActionResult> AddCarToBasket(AddCarToBasketDto addCarToBasketDto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetCarFromBasker()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto)
        {
            throw new NotImplementedException();
        }
    }
}
