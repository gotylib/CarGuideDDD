
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IBasketService
    {
        Task<IActionResult> AddCarToBasket(AddCarToBasketDto addCarToBasketDto);
        Task<IActionResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto);
        Task<IActionResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto);
        Task<IActionResult> GetCarFromBasker();
    }
}
