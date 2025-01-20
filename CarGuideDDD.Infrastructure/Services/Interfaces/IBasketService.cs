
using CarGuideDDD.Core.DtObjects;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IBasketService
    {
        Task<IActionResult> AddCarToBasket(AddCarToBasketDto addCarToBasketDto, List<string> roles, string name);
        Task<IActionResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name);
        Task<IActionResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name);
        Task<IActionResult> GetCarFromBasker(List<string> roles, string name);
    }
}
