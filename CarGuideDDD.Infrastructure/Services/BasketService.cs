using CarGuideDDD.Core.DomainObjects;
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
        public async Task<IActionResult> AddCarToBasket(AddCarToBasketDto addCarToBasketDto, List<string> roles, string name)
        {
            switch (Car.CDUToBasket(addCarToBasketDto, roles, name))
            {
                case RoleBasketCDU.Manager:
                    return new BadRequestObjectResult("Менеджерам эта функция не доступна");
                case RoleBasketCDU.NonAdmin:
                    return new BadRequestObjectResult("Вы не админ по этому не можете менять корзины других пользователей");
                case RoleBasketCDU.Default:
                    if (await _repository.AddCarToBasket(addCarToBasketDto))
                    {
                        return new OkObjectResult("Машина была добавлена");
                    }
                    break;
            }

            return new StatusCodeResult(500);
        }

        public async Task<IActionResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name)
        {
            switch (Car.CDUToBasket(deleteCarFromBasketDto, roles, name))
            {
                case RoleBasketCDU.Manager:
                    return new BadRequestObjectResult("Менеджерам эта функция не доступна");
                case RoleBasketCDU.NonAdmin:
                    return new BadRequestObjectResult("Вы не админ по этому не можете менять корзины других пользователей");
                case RoleBasketCDU.Default:
                    if (await _repository.DeleteCar(deleteCarFromBasketDto))
                    {
                        return new OkObjectResult("Машина была удалена из корзины");
                    }
                    break;
            }

            return new StatusCodeResult(500);
        }

        public async Task<IActionResult> GetCarFromBasker(List<string> roles, string name)
        {
            switch(Car.GetBasket(roles, name))
            {
                case RoleBasketGet.Manager:
                    return new BadRequestObjectResult("Менеджерам эта функция не доступна");
                case RoleBasketGet.Admin:
                    var resultAdmin = await _repository.GetAllBaskets();

                    return resultAdmin.IsSuccessful
                        ? new OkObjectResult(resultAdmin.Value)
                        : new BadRequestObjectResult(resultAdmin.Error);
                case RoleBasketGet.User:
                    var result = await _repository.GetBasket(name);

                    return result.IsSuccessful
                        ? new OkObjectResult(result.Value)
                        : new BadRequestObjectResult(result.Error);
            }
            return new StatusCodeResult(500);
        }

        public async Task<IActionResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name)
        {
            switch (Car.CDUToBasket(updateColorDto, roles, name))
            {
                case RoleBasketCDU.Manager:
                    return new BadRequestObjectResult("Менеджерам эта функция не доступна");
                case RoleBasketCDU.NonAdmin:
                    return new BadRequestObjectResult("Вы не админ по этому не можете менять корзины других пользователей");
                case RoleBasketCDU.Default:
                    if (await _repository.UpdateCarColor(updateColorDto))
                    {
                        return new OkObjectResult("Цвет машины был обновлен");
                    }
                    break;
            }

            return new StatusCodeResult(500);
        }
    }
}
