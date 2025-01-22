using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.DomainObjects;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
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
        public async Task<ServiceResult> AddCarToBasket(AddCarToBasketDto addCarToBasketDto, List<string> roles, string name)
        {
            
            switch (Car.CDUToBasket(addCarToBasketDto, roles, name))
            {
                case RoleBasketCDU.Manager:
                    return ServiceResult.BadRequest("Менеджерам эта функция не доступна",403);
                case RoleBasketCDU.NonAdmin:
                    return ServiceResult.BadRequest("Вы не админ по этому не можете менять корзины других пользователей", 403);
                case RoleBasketCDU.Default:
                    if (await _repository.AddCarToBasket(addCarToBasketDto))
                    {
                        return ServiceResult.Ok("Машина была добавлена");
                    }
                    break;
            }



            return ServiceResult.ServerError();
        }

        public async Task<ServiceResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name)
        {
            switch (Car.CDUToBasket(deleteCarFromBasketDto, roles, name))
            {
                case RoleBasketCDU.Manager:
                    return ServiceResult.BadRequest("Менеджерам эта функция не доступна",403);
                case RoleBasketCDU.NonAdmin:
                    return ServiceResult.BadRequest("Вы не админ по этому не можете менять корзины других пользователей", 403);
                case RoleBasketCDU.Default:
                    if (await _repository.DeleteCar(deleteCarFromBasketDto))
                    {
                        return ServiceResult.Ok("Машина была удалена из корзины");
                    }
                    break;
            }

            return ServiceResult.ServerError();
        }

        public async Task<ServiceResultGet<EntityUser,Exception,EntityBasket>> GetCarFromBasker(List<string> roles, string name)
        {
            switch(Car.GetBasket(roles, name))
            {
                case RoleBasketGet.Manager:
                    return ServiceResultGet<EntityUser, Exception, EntityBasket>.BadRequest("Менеджерам эта функция не доступна",403);
                case RoleBasketGet.Admin:
                    var resultAdmin = await _repository.GetAllBaskets();

                    return resultAdmin.IsSuccessful
                        ? ServiceResultGet < EntityUser, Exception, EntityBasket>.IEnumerableResult(resultAdmin.Value)
                        : ServiceResultGet<EntityUser, Exception, EntityBasket>.ErrorResult(resultAdmin.Error);
                case RoleBasketGet.User:
                    var result = await _repository.GetBasket(name);

                    return result.IsSuccessful
                        ? ServiceResultGet<EntityUser, Exception, EntityBasket>.SimpleResult(result.Value)
                        : ServiceResultGet<EntityUser, Exception, EntityBasket>.ErrorResult(result.Error); ;
            }
            return ServiceResultGet<EntityUser, Exception, EntityBasket>.ServerError();
        }

        public async Task<ServiceResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name)
        {
            switch (Car.CDUToBasket(updateColorDto, roles, name))
            {
                case RoleBasketCDU.Manager:
                    return ServiceResult.BadRequest("Менеджерам эта функция не доступна",403);
                case RoleBasketCDU.NonAdmin:
                    return ServiceResult.BadRequest("Вы не админ по этому не можете менять корзины других пользователей", 403);
                case RoleBasketCDU.Default:
                    if (await _repository.UpdateCarColor(updateColorDto))
                    {
                        return ServiceResult.Ok("Цвет машины был обновлен");
                    }
                    break;
            }

            return ServiceResult.ServerError();
        }
    }
}
