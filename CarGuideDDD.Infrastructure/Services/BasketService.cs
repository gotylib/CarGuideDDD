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
            if (roles.Contains("Manager")) { return new BadRequestObjectResult("Менеджерам эта функция не доступна"); }

            if (roles.Contains("Admin") == false || addCarToBasketDto.Username != name)
            {
                return new BadRequestObjectResult("Вы не админ по этому не можете менять корзины других пользователей");
            }

            if(await _repository.AddCarToBasket(addCarToBasketDto))
            {
                return new OkObjectResult("Машина была добавлена");
            }

            return new StatusCodeResult(500);
        }

        public async Task<IActionResult> DeleteCarFromBasket(DeleteCarFromBasketDto deleteCarFromBasketDto, List<string> roles, string name)
        {
            if (roles.Contains("Manager"))
            {
                return new BadRequestObjectResult("Менеджерам эта функция не доступна");
            }

            if (!roles.Contains("Admin") || deleteCarFromBasketDto.UserName != name)
            {
                return new BadRequestObjectResult("Вы не админ, поэтому не можете удалять машины из корзин других пользователей");
            }

            if (await _repository.DeleteCar(deleteCarFromBasketDto))
            {
                return new OkObjectResult("Машина была удалена из корзины");
            }

            return new StatusCodeResult(500);
        }

        public async Task<IActionResult> GetCarFromBasker(List<string> roles, string name)
        {
            if (roles.Contains("Manager"))
            {
                return new BadRequestObjectResult("Менеджерам эта функция не доступна");
            }

            if (roles.Contains("Admin"))
            {
                var result = await _repository.GetAllBaskets();

                return result.IsSuccessful
                    ? new OkObjectResult(result.Value)
                    : new BadRequestObjectResult(result.Error);
            }
            else
            {
                var result = await _repository.GetBasket(name);

                return result.IsSuccessful
                    ? new OkObjectResult(result.Value)
                    : new BadRequestObjectResult(result.Error);
            }
            
            
        }

        public async Task<IActionResult> UpdateColorToCarFromBasket(UpdateColorDto updateColorDto, List<string> roles, string name)
        {
            if (roles.Contains("Manager"))
            {
                return new BadRequestObjectResult("Менеджерам эта функция не доступна");
            }

            if (!roles.Contains("Admin") || updateColorDto.UserName != name)
            {
                return new BadRequestObjectResult("Вы не админ, поэтому не можете изменять корзины других пользователей");
            }

            if (await _repository.UpdateCarColor(updateColorDto))
            {
                return new OkObjectResult("Цвет машины был обновлен");
            }

            return new StatusCodeResult(500);
        }
    }
}
