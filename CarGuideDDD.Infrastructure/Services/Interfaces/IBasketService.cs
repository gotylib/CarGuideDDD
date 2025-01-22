
using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CarGuideDDD.Infrastructure.Services.Interfaces
{
    public interface IBasketService
    {
        Task<ServiceResult<VoidDto, Exception, AddCarToBasketDto>> AddCarToBasket(AddCarToBasketDto addCarToBasketDto, List<string> roles, string name);
        Task<ServiceResult<VoidDto, Exception, UpdateColorDto>> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name);
        Task<ServiceResult<VoidDto, Exception, DeleteCarFromBasketDto>> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name);
        Task<ServiceResult<EntityUser, Exception, EntityBasket>> GetCarFromBasker(List<string> roles, string name);
    }
}
