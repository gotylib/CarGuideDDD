
using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IBasketService
    {
        Task<ServiceResult> AddCarToBasket(AddCarToBasketDto addCarToBasketDto, List<string> roles, string name);
        Task<ServiceResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name);
        Task<ServiceResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name);
        Task<ServiceResultGet<EntityUser, Exception, EntityBasket>> GetCarFromBasker(List<string> roles, string name);
    }
}
